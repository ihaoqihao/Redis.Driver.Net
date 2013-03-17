using System;
using System.Text;
using Sodao.FastSocket.Client.Protocol;
using Sodao.FastSocket.SocketBase;

namespace Redis.Driver
{
    /// <summary>
    /// redis协议
    /// </summary>
    public sealed class RedisProtocol : IProtocol<IRedisReply>
    {
        #region IProtocol Members
        /// <summary>
        /// find response
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        /// <exception cref="BadProtocolException">未能识别的协议</exception>
        public IRedisReply FindResponse(IConnection connection, byte[] buffer, out int readed)
        {
            if (buffer == null || buffer.Length == 0)
            {
                readed = 0;
                return null;
            }

            switch (buffer[0])
            {
                case 43://'+'
                    return this.FindStatus(connection, buffer, out readed);
                case 45://'-'
                    return this.FindError(connection, buffer, out readed);
                case 58://':'
                    return this.FindInteger(connection, buffer, out readed);
                case 36://'$'
                    return this.FindBulk(connection, buffer, out readed);
                case 42://'*'
                    return this.FindMultiBulk(connection, buffer, out readed);
                default:
                    throw new BadProtocolException();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// get seqID
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        static private int GetSeqID(IConnection connection)
        {
            return (connection.UserData as IRedisReplyQueue).Dequeue();
        }
        /// <summary>
        /// find status reply
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        private StatusReply FindStatus(IConnection connection, byte[] buffer, out int readed)
        {
            for (int i = 1, l = buffer.Length; i < l; i++)
                if (buffer[i] == 13 && i + 1 < l && buffer[i + 1] == 10)
                {
                    readed = i + 2;
                    return new StatusReply(GetSeqID(connection), Encoding.UTF8.GetString(buffer, 1, i - 1));
                }

            readed = 0;
            return null;
        }
        /// <summary>
        /// find error reply
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        private ErrorReply FindError(IConnection connection, byte[] buffer, out int readed)
        {
            for (int i = 1, l = buffer.Length; i < l; i++)
                if (buffer[i] == 13 && i + 1 < l && buffer[i + 1] == 10)
                {
                    readed = i + 2;
                    return new ErrorReply(GetSeqID(connection), Encoding.UTF8.GetString(buffer, 1, i - 1));
                }

            readed = 0;
            return null;
        }
        /// <summary>
        /// find integer reply
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        private IntegerReply FindInteger(IConnection connection, byte[] buffer, out int readed)
        {
            var prefixed = GetPrefixedLength(buffer, 0);
            if (prefixed.OverIndex == -1)
            {
                readed = 0;
                return null;
            }

            readed = prefixed.OverIndex + 1;
            if (readed > buffer.Length)
            {
                readed = 0;
                return null;
            }
            return new IntegerReply(GetSeqID(connection), prefixed.Value);
        }
        /// <summary>
        /// find bulk reply
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        private BulkReplies FindBulk(IConnection connection, byte[] buffer, out int readed)
        {
            //find bulk length
            var prefixed = GetPrefixedLength(buffer, 0);
            if (prefixed.OverIndex == -1)
            {
                readed = 0;
                return null;
            }

            if (prefixed.Value < 1)
            {
                readed = prefixed.OverIndex + 1;
                if (readed > buffer.Length)
                {
                    readed = 0;
                    return null;
                }
                return new BulkReplies(GetSeqID(connection), null);
            }

            readed = prefixed.OverIndex + prefixed.Value + 3;
            if (buffer.Length >= readed)
            {
                var payload = new byte[prefixed.Value];
                Buffer.BlockCopy(buffer, prefixed.OverIndex + 1, payload, 0, prefixed.Value);
                return new BulkReplies(GetSeqID(connection), payload);
            }

            readed = 0;
            return null;
        }
        /// <summary>
        /// find multi-bulk reply
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        private MultiBulkReplies FindMultiBulk(IConnection connection, byte[] buffer, out int readed)
        {
            var prefixed = GetPrefixedLength(buffer, 0);
            if (prefixed.OverIndex == -1)
            {
                readed = 0;
                return null;
            }

            if (prefixed.Value < 1)
            {
                readed = prefixed.OverIndex + 1;
                if (readed > buffer.Length)
                {
                    readed = 0;
                    return null;
                }
                return new MultiBulkReplies(GetSeqID(connection), null);
            }

            var arrBulk = new PrefixedLength[prefixed.Value];
            int bufferIndex = prefixed.OverIndex + 1;

            for (int i = 0, l = prefixed.Value; i < l; i++)
            {
                var childPrefixed = GetPrefixedLength(buffer, bufferIndex);
                if (childPrefixed.OverIndex == -1)
                {
                    readed = 0;
                    return null;
                }

                arrBulk[i] = childPrefixed;
                if (childPrefixed.Value < 1)
                    bufferIndex = childPrefixed.OverIndex + 1;
                else
                    bufferIndex = childPrefixed.OverIndex + childPrefixed.Value + 3;
            }

            if (bufferIndex > buffer.Length)
            {
                readed = 0;
                return null;
            }

            readed = bufferIndex;
            //copy data
            var arrPayloads = new byte[prefixed.Value][];
            for (int i = 0, l = prefixed.Value; i < l; i++)
            {
                var childPrefixed = arrBulk[i];
                if (childPrefixed.Value > 0)
                {
                    var payload = new byte[childPrefixed.Value];
                    Buffer.BlockCopy(buffer, childPrefixed.OverIndex + 1, payload, 0, childPrefixed.Value);
                    arrPayloads[i] = payload;
                }
            }
            return new MultiBulkReplies(GetSeqID(connection), arrPayloads);
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// get prefixed length
        /// </summary>
        /// <param name="buffer">如"$7\r\n"</param>
        /// <param name="index"></param>
        /// <returns>if not found, return {OverIndex=-1,Value=-1}</returns>
        /// <exception cref="ArgumentOutOfRangeException">start less than 0.</exception>
        static private PrefixedLength GetPrefixedLength(byte[] buffer, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "index less than 0.");

            if (buffer.Length - index < 2)
                return new PrefixedLength(-1, -1);

            bool isNegativeValue = buffer[index + 1] == 45;//'-' is 45
            int start = isNegativeValue ? index + 2 : index + 1;

            int intValue = 0;
            for (int i = start, l = buffer.Length; i < l; i++)
            {
                if (buffer[i] == 13)
                    return new PrefixedLength(i + 1, isNegativeValue ? -intValue : intValue);

                intValue = intValue * 10 + (buffer[i] - 48);//'0' is 48
            }

            return new PrefixedLength(-1, -1);
        }
        #endregion

        #region Private Class
        /// <summary>
        /// redis prefixed length
        /// "$7\r\n",  OverIndex=3, Value=7
        /// "$-1\r\n", OverIndex=4, Value=-1
        /// </summary>
        private struct PrefixedLength
        {
            /// <summary>
            /// prefixed length over index
            /// </summary>
            public int OverIndex;
            /// <summary>
            /// prefixed length value
            /// </summary>
            public int Value;
            /// <summary>
            /// new
            /// </summary>
            /// <param name="overIndex"></param>
            /// <param name="value"></param>
            public PrefixedLength(int overIndex, int value)
            {
                this.OverIndex = overIndex;
                this.Value = value;
            }
        }
        #endregion
    }
}
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
        /// <param name="readlength"></param>
        /// <returns></returns>
        /// <exception cref="BadProtocolException">未能识别的协议</exception>
        public IRedisReply FindResponse(IConnection connection, ArraySegment<byte> buffer, out int readlength)
        {
            switch (buffer.Array[buffer.Offset])
            {
                case 43://'+'
                    return this.FindStatus(connection, buffer, out readlength);
                case 45://'-'
                    return this.FindError(connection, buffer, out readlength);
                case 58://':'
                    return this.FindInteger(connection, buffer, out readlength);
                case 36://'$'
                    return this.FindBulk(connection, buffer, out readlength);
                case 42://'*'
                    return this.FindMultiBulk(connection, buffer, out readlength);
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
            if (connection == null)
                return -1;

            return (connection.UserData as IRedisReplyQueue).Dequeue();
        }
        /// <summary>
        /// find status reply
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        private StatusReply FindStatus(IConnection connection, ArraySegment<byte> buffer, out int readlength)
        {
            var payload = buffer.Array;
            for (int i = buffer.Offset + 1, l = buffer.Offset + buffer.Count; i < l; i++)
                if (payload[i] == 13 && i + 1 < l && payload[i + 1] == 10)
                {
                    readlength = i + 2 - buffer.Offset;
                    return new StatusReply(GetSeqID(connection), Encoding.UTF8.GetString(payload, buffer.Offset + 1, readlength - 3));
                }

            readlength = 0;
            return null;
        }
        /// <summary>
        /// find error reply
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        private ErrorReply FindError(IConnection connection, ArraySegment<byte> buffer, out int readlength)
        {
            var payload = buffer.Array;
            for (int i = buffer.Offset + 1, l = buffer.Offset + buffer.Count; i < l; i++)
                if (payload[i] == 13 && i + 1 < l && payload[i + 1] == 10)
                {
                    readlength = i + 2 - buffer.Offset;
                    return new ErrorReply(GetSeqID(connection), Encoding.UTF8.GetString(payload, buffer.Offset + 1, readlength - 3));
                }

            readlength = 0;
            return null;
        }
        /// <summary>
        /// find integer reply
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        private IntegerReply FindInteger(IConnection connection, ArraySegment<byte> buffer, out int readlength)
        {
            var prefixed = GetPrefixedLength(buffer);
            if (prefixed.OverIndex == -1)
            {
                readlength = 0;
                return null;
            }

            readlength = prefixed.OverIndex + 1 - buffer.Offset;
            if (readlength > buffer.Count)
            {
                readlength = 0;
                return null;
            }
            return new IntegerReply(GetSeqID(connection), prefixed.Value);
        }
        /// <summary>
        /// find bulk reply
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns> 
        private BulkReplies FindBulk(IConnection connection, ArraySegment<byte> buffer, out int readlength)
        {
            //find bulk length
            var prefixed = GetPrefixedLength(buffer);
            if (prefixed.OverIndex == -1)
            {
                readlength = 0;
                return null;
            }

            if (prefixed.Value < 1)
            {
                readlength = prefixed.OverIndex + 1 - buffer.Offset;
                if (readlength > buffer.Count)
                {
                    readlength = 0;
                    return null;
                }
                return new BulkReplies(GetSeqID(connection), null);
            }

            readlength = prefixed.OverIndex + prefixed.Value + 3 - buffer.Offset;
            if (buffer.Count >= readlength)
            {
                var payload = new byte[prefixed.Value];
                Buffer.BlockCopy(buffer.Array, prefixed.OverIndex + 1, payload, 0, prefixed.Value);
                return new BulkReplies(GetSeqID(connection), payload);
            }

            readlength = 0;
            return null;
        }
        /// <summary>
        /// find multi-bulk reply
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        private MultiBulkReplies FindMultiBulk(IConnection connection, ArraySegment<byte> buffer, out int readlength)
        {
            var prefixed = GetPrefixedLength(buffer);
            if (prefixed.OverIndex == -1)
            {
                readlength = 0;
                return null;
            }

            if (prefixed.Value < 1)
            {
                readlength = 5;
                if (readlength > buffer.Count)
                {
                    readlength = 0;
                    return null;
                }
                return new MultiBulkReplies(GetSeqID(connection), null);
            }

            var arrBulk = new PrefixedLength[prefixed.Value];
            var currentOffset = prefixed.OverIndex + 1;
            var nextBuffer = new ArraySegment<byte>(buffer.Array, currentOffset, buffer.Count - currentOffset + buffer.Offset);

            for (int i = 0, l = prefixed.Value; i < l; i++)
            {
                var childPrefixed = GetPrefixedLength(nextBuffer);
                if (childPrefixed.OverIndex == -1)
                {
                    readlength = 0;
                    return null;
                }

                arrBulk[i] = childPrefixed;

                if (childPrefixed.Value < 1)
                    currentOffset = childPrefixed.OverIndex + 1;
                else
                    currentOffset = childPrefixed.OverIndex + childPrefixed.Value + 3;

                nextBuffer = new ArraySegment<byte>(buffer.Array, currentOffset, buffer.Count - currentOffset + buffer.Offset);
            }

            readlength = currentOffset - buffer.Offset;
            if (readlength > buffer.Count)
            {
                readlength = 0;
                return null;
            }

            //copy data
            var arrPayloads = new byte[prefixed.Value][];
            for (int i = 0, l = prefixed.Value; i < l; i++)
            {
                var childPrefixed = arrBulk[i];
                if (childPrefixed.Value > 0)
                {
                    var payload = new byte[childPrefixed.Value];
                    Buffer.BlockCopy(buffer.Array, childPrefixed.OverIndex + 1, payload, 0, childPrefixed.Value);
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
        /// <returns>if not found, return {OverIndex=-1,Value=-1}</returns>
        static private PrefixedLength GetPrefixedLength(ArraySegment<byte> buffer)
        {
            if (buffer.Count < 2)
                return new PrefixedLength(-1, -1);

            var payload = buffer.Array;

            bool isNegativeValue = payload[buffer.Offset + 1] == 45;//'-' is 45
            int start = isNegativeValue ? buffer.Offset + 2 : buffer.Offset + 1;

            int intValue = 0;
            for (int i = start, l = buffer.Offset + buffer.Count; i < l; i++)
            {
                if (payload[i] == 13)
                    return new PrefixedLength(i + 1, isNegativeValue ? -intValue : intValue);

                intValue = intValue * 10 + (payload[i] - 48);//'0' is 48
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
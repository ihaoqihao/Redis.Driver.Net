using System;
using System.Text;
using Sodao.FastSocket.Client.Protocol;
using Sodao.FastSocket.SocketBase;

namespace Redis.Driver
{
    /// <summary>
    /// redis协议
    /// </summary>
    public sealed class RedisProtocol : IProtocol<RedisResponse>
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
        public RedisResponse FindResponse(IConnection connection, ArraySegment<byte> buffer, out int readlength)
        {
            IRedisReply reply = null;
            switch (buffer.Array[buffer.Offset])
            {
                case 43: reply = this.FindStatus(buffer, out readlength); break;    //'+'
                case 45: reply = this.FindError(buffer, out readlength); break;     //'-'
                case 58: reply = this.FindInteger(buffer, out readlength); break;   //':'
                case 36: reply = this.FindBulk(buffer, out readlength); break;      //'$'
                case 42: reply = this.FindMultiBulk(buffer, out readlength); break; //'*'
                default: throw new BadProtocolException();
            }
            if (reply == null) return null;
            return new RedisResponse(GetSeqID(connection), reply);
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
            if (connection == null || connection.UserData == null) return -1;
            return (connection.UserData as IRedisReplyQueue).Dequeue();
        }
        /// <summary>
        /// find status reply
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        private StatusReply FindStatus(ArraySegment<byte> buffer, out int readlength)
        {
            var payload = buffer.Array;
            for (int i = buffer.Offset + 1, l = buffer.Offset + buffer.Count; i < l; i++)
            {
                if (payload[i] == 13 && i + 1 < l && payload[i + 1] == 10)
                {
                    readlength = i + 2 - buffer.Offset;
                    return new StatusReply(Encoding.UTF8.GetString(payload, buffer.Offset + 1, readlength - 3));
                }
            }
            readlength = 0;
            return null;
        }
        /// <summary>
        /// find error reply
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        private ErrorReply FindError(ArraySegment<byte> buffer, out int readlength)
        {
            var payload = buffer.Array;
            for (int i = buffer.Offset + 1, l = buffer.Offset + buffer.Count; i < l; i++)
            {
                if (payload[i] == 13 && i + 1 < l && payload[i + 1] == 10)
                {
                    readlength = i + 2 - buffer.Offset;
                    return new ErrorReply(Encoding.UTF8.GetString(payload, buffer.Offset + 1, readlength - 3));
                }
            }
            readlength = 0;
            return null;
        }
        /// <summary>
        /// find integer reply
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        private IntegerReply FindInteger(ArraySegment<byte> buffer, out int readlength)
        {
            var prefixed = GetPrefixedLength(buffer);
            if (prefixed.OverIndex == -1) { readlength = 0; return null; }

            readlength = prefixed.OverIndex + 1 - buffer.Offset;
            return new IntegerReply(prefixed.Value);
        }
        /// <summary>
        /// find bulk reply
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns> 
        private BulkReplies FindBulk(ArraySegment<byte> buffer, out int readlength)
        {
            //find bulk length
            var prefixed = GetPrefixedLength(buffer);
            if (prefixed.OverIndex == -1) { readlength = 0; return null; }

            if (prefixed.Value < 1)
            {
                readlength = prefixed.OverIndex + 1 - buffer.Offset;
                return new BulkReplies(null);
            }

            readlength = prefixed.OverIndex + prefixed.Value + 3 - buffer.Offset;
            if (buffer.Count >= readlength)
            {
                var payload = new byte[prefixed.Value];
                Buffer.BlockCopy(buffer.Array, prefixed.OverIndex + 1, payload, 0, prefixed.Value);
                return new BulkReplies(payload);
            }

            readlength = 0;
            return null;
        }
        /// <summary>
        /// find multi-bulk reply
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        private MultiBulkReplies FindMultiBulk(ArraySegment<byte> buffer, out int readlength)
        {
            var prefixed = GetPrefixedLength(buffer);
            if (prefixed.OverIndex == -1) { readlength = 0; return null; }

            if (prefixed.Value < 1)
            {
                readlength = prefixed.OverIndex + 1 - buffer.Offset;
                return new MultiBulkReplies(null);
            }

            var bulkCount = buffer.Count - (prefixed.OverIndex + 1 - buffer.Offset);
            if (bulkCount < 1) { readlength = 0; return null; }
            var bulkBuffer = new ArraySegment<byte>(buffer.Array, prefixed.OverIndex + 1, bulkCount);

            var replies = new IRedisReply[prefixed.Value];
            int childReadLength = 0;
            for (int i = 0, l = prefixed.Value; i < l; i++)
            {
                IRedisReply reply = null;
                switch (bulkBuffer.Array[bulkBuffer.Offset])
                {
                    case 43: reply = this.FindStatus(bulkBuffer, out childReadLength); break;   //'+'
                    case 45: reply = this.FindError(bulkBuffer, out childReadLength); break;    //'-'
                    case 58: reply = this.FindInteger(bulkBuffer, out childReadLength); break;  //':'
                    case 36: reply = this.FindBulk(bulkBuffer, out childReadLength); break;     //'$'
                    default: throw new BadProtocolException();
                }

                if (reply == null) { readlength = 0; return null; }
                replies[i] = reply;

                if (i < l - 1)
                {
                    var childBulkCount = bulkBuffer.Count - childReadLength;
                    if (childBulkCount < 1) { readlength = 0; return null; }
                    bulkBuffer = new ArraySegment<byte>(buffer.Array, bulkBuffer.Offset + childReadLength, childBulkCount);
                }
            }

            readlength = bulkBuffer.Offset + childReadLength - buffer.Offset;
            return new MultiBulkReplies(replies);
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
            if (buffer.Count < 2) return new PrefixedLength(-1, -1);

            bool isNegative = buffer.Array[buffer.Offset + 1] == 45;//'-' is 45
            int start = isNegative ? buffer.Offset + 2 : buffer.Offset + 1;

            int intValue = 0;
            for (int i = start, l = buffer.Offset + buffer.Count; i < l; i++)
            {
                if (buffer.Array[i] == 13 && i + 1 < l && buffer.Array[i + 1] == 10)
                    return new PrefixedLength(i + 1, isNegative ? -intValue : intValue);

                intValue = intValue * 10 + (buffer.Array[i] - 48);//'0' is 48
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
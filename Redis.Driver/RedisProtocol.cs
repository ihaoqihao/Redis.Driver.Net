using System;
using Sodao.FastSocket.Client.Protocol;
using System.Text;

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
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        /// <exception cref="BadProtocolException">未能识别的协议</exception>
        public IRedisReply FindResponse(byte[] buffer, out int readed)
        {
            if (buffer == null || buffer.Length == 0)
            {
                readed = 0;
                return null;
            }

            switch ((char)buffer[0])
            {
                case '+':
                    return this.FindStatus(buffer, out readed);
                case '-':
                    return this.FindError(buffer, out readed);
                case ':':
                    return this.FindInteger(buffer, out readed);
                case '$':
                    return this.FindBulk(buffer, out readed);
                case '*':
                    return this.FindMultiBulk(buffer, out readed);
                default:
                    throw new BadProtocolException();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// find status reply
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        private StatusReply FindStatus(byte[] buffer, out int readed)
        {
            int length = buffer.Length;
            if (length < 4)
            {
                readed = 0;
                return null;
            }
            for (int i = 1; i < length; i++)
            {
                if (buffer[i] == 13 && i + 1 < length && buffer[i + 1] == 10)
                {
                    readed = i + 1;
                    return new StatusReply(Encoding.UTF8.GetString(buffer, 1, i - 1));
                }
            }

            readed = 0;
            return null;
        }
        /// <summary>
        /// find error reply
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        private ErrorReply FindError(byte[] buffer, out int readed)
        {
            int length = buffer.Length;
            if (length < 4)
            {
                readed = 0;
                return null;
            }
            for (int i = 1; i < length; i++)
            {
                if (buffer[i] == 13 && i + 1 < length && buffer[i + 1] == 10)
                {
                    readed = i + 1;
                    return new ErrorReply(Encoding.UTF8.GetString(buffer, 1, i - 1));
                }
            }

            readed = 0;
            return null;
        }
        /// <summary>
        /// find integer reply
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        private IntegerReply FindInteger(byte[] buffer, out int readed)
        {
            int length = buffer.Length;
            if (length < 4)
            {
                readed = 0;
                return null;
            }
            for (int i = 1; i < length; i++)
            {
                if (buffer[i] == 13 && i + 1 < length && buffer[i + 1] == 10)
                {
                    readed = i + 1;
                    int value = 0;
                    if (!int.TryParse(Encoding.UTF8.GetString(buffer, 1, i - 1), out value))
                        throw new BadProtocolException();

                    return new IntegerReply(value);
                }
            }

            readed = 0;
            return null;
        }
        /// <summary>
        /// find bulk reply
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        private BulkReplies FindBulk(byte[] buffer, out int readed)
        {
            int length = buffer.Length;
            if (length < 3)
            {
                readed = 0;
                return null;
            }
            //nil
            if (buffer[1] == '-' && buffer[2] == '1')
            {
                readed = 3;
                return new BulkReplies(null);
            }

            if (length < 4)
            {
                readed = 0;
                return null;
            }
            for (int i = 1; i < length; i++)
            {
                if (buffer[i] == 13 && i + 1 < length && buffer[i + 1] == 10)
                {
                    int payloadLength = 0;
                    if (!int.TryParse(Encoding.UTF8.GetString(buffer, 1, i - 1), out payloadLength))
                        throw new BadProtocolException();

                    var leastLength = payloadLength + i + 4;
                    if (length >= leastLength)
                    {
                        readed = leastLength;
                        var payload = new byte[payloadLength];
                        Buffer.BlockCopy(buffer, i + 2, payload, 0, payloadLength);
                        return new BulkReplies(payload);
                    }
                }
            }

            readed = 0;
            return null;
        }
        /// <summary>
        /// find multi-bulk reply
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        private MultiBulkReplies FindMultiBulk(byte[] buffer, out int readed)
        {
            readed = 0;
            return null;
        }
        #endregion
    }
}
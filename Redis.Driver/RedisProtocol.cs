using System;
using Sodao.FastSocket.Client.Protocol;

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
            readed = 0;
            return null;
        }
        /// <summary>
        /// find bulk reply
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readed"></param>
        /// <returns></returns>
        private IntegerReply FindBulk(byte[] buffer, out int readed)
        {
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
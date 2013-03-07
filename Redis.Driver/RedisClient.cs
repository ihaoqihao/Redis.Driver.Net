using System;
using System.Threading.Tasks;
using Sodao.FastSocket.Client;

namespace Redis.Driver
{
    /// <summary>
    /// redis client
    /// </summary>
    public sealed class RedisClient : SocketClient<IRedisReply>
    {
        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="socketBufferSize">socket缓存区大小</param>
        /// <param name="messageBufferSize">消息缓存区大小</param>
        /// <param name="millisecondsSendTimeout">发送超时时间</param>
        /// <param name="millisecondsReceiveTimeout">接收超时时间</param>
        public RedisClient(int socketBufferSize,
            int messageBufferSize,
            int millisecondsSendTimeout,
            int millisecondsReceiveTimeout)
            : base(null,
            new RedisProtocol(),
            new RequestReceivingQueue<IRedisReply>(millisecondsReceiveTimeout),
            socketBufferSize,
            messageBufferSize,
            millisecondsSendTimeout)
        {
        }
        #endregion
    }
}
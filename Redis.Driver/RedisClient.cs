using System;
using System.Threading.Tasks;
using Sodao.FastSocket.Client;

namespace Redis.Driver
{
    /// <summary>
    /// redis client
    /// </summary>
    public sealed class RedisClient : SocketClient<IRedisReply>,
        IStringCommands
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

        #region Public Properties
        /// <summary>
        /// Strings
        /// </summary>
        public IStringCommands Strings
        {
            get { return this; }
        }
        #endregion

        #region IStringCommands Members
        /// <summary>
        /// If key already exists and is a string, 
        /// this command appends the value at the end of the string. 
        /// If key does not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>the length of the string after the append operation.</returns>
        /// <remarks>http://redis.io/commands/append</remarks>
        Task<int> IStringCommands.Append(string key, string value)
        {
            return this.ExecuteInt(new RedisRequest(3).AddArgument("APPEND").AddArgument(key).AddArgument(value));
        }
        /// <summary>
        /// If key already exists and is a string, 
        /// this command appends the value at the end of the string. 
        /// If key does not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>the length of the string after the append operation.</returns>
        /// <remarks>http://redis.io/commands/append</remarks>
        Task<int> IStringCommands.Append(string key, byte[] value)
        {
            return this.ExecuteInt(new RedisRequest(3).AddArgument("APPEND").AddArgument(key).AddArgument(value));
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// execute int
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Task<int> ExecuteInt(RedisRequest request)
        {
            var source = new TaskCompletionSource<int>();
            this.Send(new Request<IRedisReply>(null, null, 0, request.ToPayload(), null,
                ex => source.TrySetException(ex),
                reply =>
                {
                    var intReply = reply as IntegerReply;
                    if (intReply != null)
                    {
                        source.TrySetResult(intReply.Value);
                        return;
                    }

                    if (reply is ErrorReply)
                    {
                        source.TrySetException((reply as ErrorReply).Error());
                        return;
                    }

                    source.TrySetException(new RedisException("failed parse the reply"));
                }));
            return source.Task;
        }
        #endregion
    }
}
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
        Task<int> IStringCommands.Append(string key, string value)
        {
            return this.ExecuteInt(new RedisRequest(3).AddArgument("APPEND")
                .AddArgument(key).AddArgument(value));
        }
        /// <summary>
        /// If key already exists and is a string, 
        /// this command appends the value at the end of the string. 
        /// If key does not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>the length of the string after the append operation.</returns>
        Task<int> IStringCommands.Append(string key, byte[] value)
        {
            return this.ExecuteInt(new RedisRequest(3).AddArgument("APPEND")
                .AddArgument(key).AddArgument(value));
        }
        /// <summary>
        /// When offset is beyond the string length, 
        /// the string is assumed to be a contiguous space with 0 bits. When key does not exist it is assumed to be an empty string, 
        /// so offset is always out of range and the value is also assumed to be a contiguous space with 0 bits.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <returns>Returns the bit value at offset in the string value stored at key.</returns>
        Task<int> IStringCommands.GetBit(string key, int offset)
        {
            return this.ExecuteInt(new RedisRequest(3).AddArgument("GETBIT")
                .AddArgument(key).AddArgument(offset));
        }
        /// <summary>
        /// Returns the values of all specified keys. 
        /// For every key that does not hold a string value or does not exist, 
        /// the special value nil is returned. Because of this, the operation never fails.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>list of values at the specified keys.</returns>
        Task<byte[][]> IStringCommands.Get(params string[] keys)
        {
            return this.ExecuteMultiBytes(new RedisRequest(keys.Length + 1).AddArgument("MGET")
                .AddArgument(keys));
        }
        /// <summary>
        /// Get the value of key. 
        /// If the key does not exist the special value nil is returned. 
        /// An error is returned if the value stored at key is not a string, because GET only handles string values.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>the value of key, or nil when key does not exist.</returns>
        Task<byte[]> IStringCommands.Get(string key)
        {
            return this.ExecuteBytes(new RedisRequest(2).AddArgument("GET")
                .AddArgument(key));
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
        /// <summary>
        /// execute multi bytes
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Task<byte[][]> ExecuteMultiBytes(RedisRequest request)
        {
            var source = new TaskCompletionSource<byte[][]>();
            this.Send(new Request<IRedisReply>(null, null, 0, request.ToPayload(), null,
                ex => source.TrySetException(ex),
                reply =>
                {
                    var mbReeply = reply as MultiBulkReplies;
                    if (mbReeply != null)
                    {
                        source.TrySetResult(mbReeply.Payloads);
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
        /// <summary>
        /// execute bytes
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Task<byte[]> ExecuteBytes(RedisRequest request)
        {
            var source = new TaskCompletionSource<byte[]>();
            this.Send(new Request<IRedisReply>(null, null, 0, request.ToPayload(), null,
                ex => source.TrySetException(ex),
                reply =>
                {
                    var mbReeply = reply as BulkReplies;
                    if (mbReeply != null)
                    {
                        source.TrySetResult(mbReeply.Payload);
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
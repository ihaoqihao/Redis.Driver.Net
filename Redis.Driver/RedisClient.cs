using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodao.FastSocket.Client;
using Sodao.FastSocket.SocketBase;

namespace Redis.Driver
{
    /// <summary>
    /// redis client
    /// </summary>
    public sealed class RedisClient : PooledSocketClient<RedisResponse>, IStringCommands, IKeyCommands, IHashCommands
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
            : base(new RedisProtocol(),
            socketBufferSize,
            messageBufferSize,
            millisecondsSendTimeout,
            millisecondsReceiveTimeout)
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
        /// <summary>
        /// keys
        /// </summary>
        public IKeyCommands Keys
        {
            get { return this; }
        }
        /// <summary>
        /// hashes
        /// </summary>
        public IHashCommands Hashes
        {
            get { return this; }
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// OnConnected
        /// </summary>
        /// <param name="connection"></param>
        protected override void OnConnected(IConnection connection)
        {
            connection.UserData = new DefaultRedisReplyQueue();
            base.OnConnected(connection);
        }
        /// <summary>
        /// OnStartSending
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        protected override void OnStartSending(IConnection connection, Packet packet)
        {
            (connection.UserData as IRedisReplyQueue).Enqueue((packet as Request<RedisResponse>).SeqID);
            base.OnStartSending(connection, packet);
        }
        /// <summary>
        /// OnSendCallback
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="e"></param>
        protected override void OnSendCallback(IConnection connection, SendCallbackEventArgs e)
        {
            if (e.Status != SendCallbackStatus.Success) connection.BeginDisconnect();
            base.OnSendCallback(connection, e);
        }
        #endregion

        #region IKeyCommands Members
        /// <summary>
        /// Removes the specified keys. A key is ignored if it does not exist.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="asyncState"></param>
        /// <returns>The number of keys that were removed.</returns>
        Task<int> IKeyCommands.Del(string key, object asyncState)
        {
            return this.ExecuteIntegerReply(new RedisRequest(2).AddArgument("DEL").AddArgument(key), asyncState);
        }
        /// <summary>
        /// Removes the specified keys. A key is ignored if it does not exist.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="asyncState"></param>
        /// <returns>The number of keys that were removed.</returns>
        /// <exception cref="ArgumentNullException">keys is null or empty.</exception>
        Task<int> IKeyCommands.Del(string[] keys, object asyncState)
        {
            if (keys == null || keys.Length == 0) throw new ArgumentNullException("keys", "keys is null or empty.");
            return this.ExecuteIntegerReply(new RedisRequest(keys.Length + 1).AddArgument("DEL").AddArgument(keys), asyncState);
        }
        /// <summary>
        /// Returns all keys matching pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="asyncState"></param>
        /// <returns>list of keys matching pattern.</returns>
        Task<string[]> IKeyCommands.Keys(string pattern, object asyncState)
        {
            return this.ExecuteMultiBulkReplies(new RedisRequest(2).AddArgument("KEYS").AddArgument(pattern), asyncState)
                .ContinueWith(c =>
                {
                    if (c.IsFaulted) return null;
                    if (c.Result == null || c.Result.Length == 0) return new string[0];
                    return c.Result.Select(p => Encoding.UTF8.GetString(p)).ToArray();
                });
        }
        /// <summary>
        /// Set a timeout on key. After the timeout has expired, the key will automatically be deleted. 
        /// A key with an associated timeout is often said to be volatile in Redis terminology.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        public Task Expire(string key, int seconds, object asyncState = null)
        {
            return this.ExecuteIntegerReply(new RedisRequest(3).AddArgument("EXPIRE").AddArgument(key).AddArgument(seconds), asyncState);
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
        /// <param name="asyncState"></param>
        /// <returns>the length of the string after the append operation.</returns>
        Task<int> IStringCommands.Append(string key, string value, object asyncState)
        {
            return this.ExecuteIntegerReply(new RedisRequest(3).AddArgument("APPEND").AddArgument(key).AddArgument(value), asyncState);
        }
        /// <summary>
        /// If key already exists and is a string, 
        /// this command appends the value at the end of the string. 
        /// If key does not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>the length of the string after the append operation.</returns>
        Task<int> IStringCommands.Append(string key, byte[] value, object asyncState)
        {
            return this.ExecuteIntegerReply(new RedisRequest(3).AddArgument("APPEND").AddArgument(key).AddArgument(value), asyncState);
        }
        /// <summary>
        /// When offset is beyond the string length, 
        /// the string is assumed to be a contiguous space with 0 bits. When key does not exist it is assumed to be an empty string, 
        /// so offset is always out of range and the value is also assumed to be a contiguous space with 0 bits.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="asyncState"></param>
        /// <returns>Returns the bit value at offset in the string value stored at key.</returns>
        Task<int> IStringCommands.GetBit(string key, int offset, object asyncState)
        {
            return this.ExecuteIntegerReply(new RedisRequest(3).AddArgument("GETBIT").AddArgument(key).AddArgument(offset), asyncState);
        }
        /// <summary>
        /// Get the value of key. 
        /// If the key does not exist the special value nil is returned. 
        /// An error is returned if the value stored at key is not a string, because GET only handles string values.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="asyncState"></param>
        /// <returns>the value of key, or nil when key does not exist.</returns>
        Task<byte[]> IStringCommands.Get(string key, object asyncState)
        {
            return this.ExecuteBulkReplies(new RedisRequest(2).AddArgument("GET").AddArgument(key), asyncState);
        }
        /// <summary>
        /// Get the value of key. 
        /// If the key does not exist the special value nil is returned. 
        /// An error is returned if the value stored at key is not a string, because GET only handles string values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="valueFactory"></param>
        /// <param name="asyncState"></param>
        /// <returns>the value of key, or nil when key does not exist.</returns>
        Task<T> IStringCommands.Get<T>(string key, Func<byte[], T> valueFactory, object asyncState)
        {
            return this.ExecuteBulkReplies<T>(new RedisRequest(2).AddArgument("GET").AddArgument(key), valueFactory, asyncState);
        }
        /// <summary>
        /// Returns the values of all specified keys. 
        /// For every key that does not hold a string value or does not exist, 
        /// the special value nil is returned. Because of this, the operation never fails.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="asyncState"></param>
        /// <returns>list of values at the specified keys.</returns>
        /// <exception cref="ArgumentNullException">keys is null or empty.</exception>
        Task<byte[][]> IStringCommands.Get(string[] keys, object asyncState)
        {
            if (keys == null || keys.Length == 0) throw new ArgumentNullException("keys");
            return this.ExecuteMultiBulkReplies(new RedisRequest(keys.Length + 1).AddArgument("MGET").AddArgument(keys), asyncState);
        }
        /// <summary>
        /// Returns the values of all specified keys. 
        /// For every key that does not hold a string value or does not exist, 
        /// the special value nil is returned. Because of this, the operation never fails.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <param name="valueFactory"></param>
        /// <param name="asyncState"></param>
        /// <returns>list of values at the specified keys.</returns>
        Task<T[]> IStringCommands.Get<T>(string[] keys, Func<byte[], T> valueFactory, object asyncState)
        {
            if (keys == null || keys.Length == 0) throw new ArgumentNullException("keys");
            return this.ExecuteMultiBulkReplies<T>(new RedisRequest(keys.Length + 1).AddArgument("MGET").AddArgument(keys), valueFactory, asyncState);
        }
        /// <summary>
        /// Set key to hold the string value. 
        /// If key already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>always OK since SET can't fail.</returns>
        Task IStringCommands.Set(string key, string value, object asyncState)
        {
            return this.ExecuteStatusReply(new RedisRequest(3).AddArgument("SET").AddArgument(key).AddArgument(value), asyncState);
        }
        /// <summary>
        /// Set key to hold the string value. 
        /// If key already holds a value, it is overwritten, regardless of its type.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>always OK since SET can't fail.</returns>
        Task IStringCommands.Set(string key, byte[] value, object asyncState)
        {
            return this.ExecuteStatusReply(new RedisRequest(3).AddArgument("SET").AddArgument(key).AddArgument(value), asyncState);
        }
        /// <summary>
        /// Sets the given keys to their respective values. 
        /// MSET replaces existing values with new values, just as regular SET. See MSETNX if you don't want to overwrite existing values.
        /// MSET is atomic, so all given keys are set at once. 
        /// It is not possible for clients to see that some of the keys were updated while others are unchanged.
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="asyncState"></param>
        /// <returns>always OK since MSET can't fail.</returns>
        /// <exception cref="ArgumentNullException">dic is null or empty.</exception>
        Task IStringCommands.Set(Dictionary<string, byte[]> dic, object asyncState)
        {
            if (dic == null || dic.Count == 0) throw new ArgumentNullException("dic", "dic is null or empty.");

            var request = new RedisRequest(dic.Count + dic.Count + 1).AddArgument("MSET");
            foreach (var kv in dic)
                request.AddArgument(kv.Key).AddArgument(kv.Value);

            return this.ExecuteStatusReply(request, asyncState);
        }
        #endregion

        #region IHashCommands Members
        /// <summary>
        /// Removes the specified fields from the hash stored at key. 
        /// Specified fields that do not exist within this hash are ignored. 
        /// If key does not exist, it is treated as an empty hash and this command returns 0.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// the number of fields that were removed from the hash, not including specified but non existing fields.
        /// </returns>
        Task<bool> IHashCommands.Remove(string key, string field, object asyncState)
        {
            return this.ExecuteIntegerReply2(new RedisRequest(3).AddArgument("HDEL").AddArgument(key).AddArgument(field), asyncState);
        }
        /// <summary>
        /// Removes the specified fields from the hash stored at key. 
        /// Specified fields that do not exist within this hash are ignored. 
        /// If key does not exist, it is treated as an empty hash and this command returns 0.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// the number of fields that were removed from the hash, not including specified but non existing fields.
        /// </returns>
        /// <exception cref="ArgumentNullException">fields is null or empty.</exception>
        Task<int> IHashCommands.Remove(string key, string[] fields, object asyncState)
        {
            if (fields == null || fields.Length == 0) throw new ArgumentNullException("fields is null or empty.");

            var request = new RedisRequest(fields.Length + 2).AddArgument("HDEL").AddArgument(key);
            for (int i = 0, l = fields.Length; i < l; i++)
                request.AddArgument(fields[i]);

            return this.ExecuteIntegerReply(request, asyncState);
        }
        /// <summary>
        /// Returns if field is an existing field in the hash stored at key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// 1 if the hash contains field. 
        /// 0 if the hash does not contain field, or key does not exist.
        /// </returns>
        Task<bool> IHashCommands.Exists(string key, string field, object asyncState)
        {
            return this.ExecuteIntegerReply2(new RedisRequest(3).AddArgument("HEXISTS").AddArgument(key).AddArgument(field), asyncState);
        }
        /// <summary>
        /// Returns the value associated with field in the hash stored at key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// the value associated with field, 
        /// or nil when field is not present in the hash or key does not exist.
        /// </returns>
        Task<byte[]> IHashCommands.Get(string key, string field, object asyncState)
        {
            return this.ExecuteBulkReplies(new RedisRequest(3).AddArgument("HGET").AddArgument(key).AddArgument(field), asyncState);
        }
        /// <summary>
        /// Returns the values associated with the specified fields in the hash stored at key. 
        /// For every field that does not exist in the hash, a nil value is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <param name="asyncState"></param>
        /// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
        Task<byte[][]> IHashCommands.Get(string key, string[] fields, object asyncState)
        {
            if (fields == null || fields.Length == 0) throw new ArgumentNullException("fields is null or empty.");

            var request = new RedisRequest(fields.Length + 2).AddArgument("HMGET").AddArgument(key);
            for (int i = 0, l = fields.Length; i < l; i++)
                request.AddArgument(fields[i]);

            return this.ExecuteMultiBulkReplies(request, asyncState);
        }
        /// <summary>
        /// Returns all fields and values of the hash stored at key. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="asyncState"></param>
        /// <returns>list of fields and their values stored in the hash, or an empty list when key does not exist.</returns>
        Task<Dictionary<string, byte[]>> IHashCommands.GetAll(string key, object asyncState)
        {
            return this.ExecuteMultiBulkReplies(new RedisRequest(2).AddArgument("HGETALL").AddArgument(key), asyncState)
                .ContinueWith(c =>
                {
                    if (c.IsFaulted) throw c.Exception.InnerException;

                    var dic = new Dictionary<string, byte[]>(c.Result.Length / 2);
                    for (int i = 0, l = c.Result.Length; i < l; )
                    {
                        dic[Encoding.UTF8.GetString(c.Result[i])] = c.Result[i + 1];
                        i += 2;
                    }

                    return dic;
                });
        }
        /// <summary>
        /// Sets field in the hash stored at key to value. 
        /// If key does not exist, a new key holding a hash is created. 
        /// If field already exists in the hash, it is overwritten.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 
        /// 0 if field already exists in the hash and the value was updated.
        /// </returns>
        Task<int> IHashCommands.Set(string key, string field, string value, object asyncState)
        {
            return this.ExecuteIntegerReply(new RedisRequest(4).AddArgument("HSET")
                .AddArgument(key).AddArgument(field).AddArgument(value), asyncState);
        }
        /// <summary>
        /// Sets field in the hash stored at key to value. 
        /// If key does not exist, a new key holding a hash is created.
        /// If field already exists in the hash, it is overwritten.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// 1 if field is a new field in the hash and value was set. 
        /// 0 if field already exists in the hash and the value was updated.
        /// </returns>
        Task<int> IHashCommands.Set(string key, string field, byte[] value, object asyncState)
        {
            return this.ExecuteIntegerReply(new RedisRequest(4).AddArgument("HSET")
                .AddArgument(key).AddArgument(field).AddArgument(value), asyncState);
        }
        /// <summary>
        /// Sets the specified fields to their respective values in the hash stored at key. 
        /// This command overwrites any existing fields in the hash. If key does not exist, a new key holding a hash is created.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        Task IHashCommands.Set(string key, Dictionary<string, byte[]> values, object asyncState)
        {
            if (values == null || values.Count == 0) throw new ArgumentNullException("values is null or empty.");

            var request = new RedisRequest(2 + values.Count * 2).AddArgument("HMSET").AddArgument(key);
            foreach (var child in values) request.AddArgument(child.Key).AddArgument(child.Value);
            return this.ExecuteStatusReply(request, asyncState);
        }
        /// <summary>
        /// Sets field in the hash stored at key to value, only if field does not yet exist. 
        /// If key does not exist, a new key holding a hash is created. 
        /// If field already exists, this operation has no effect.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        Task<int> IHashCommands.SetIfNotExists(string key, string field, string value, object asyncState)
        {
            return this.ExecuteIntegerReply(new RedisRequest(4).AddArgument("HSETNX")
                .AddArgument(key).AddArgument(field).AddArgument(value), asyncState);
        }
        /// <summary>
        /// Sets field in the hash stored at key to value, only if field does not yet exist. 
        /// If key does not exist, a new key holding a hash is created. 
        /// If field already exists, this operation has no effect.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        Task<int> IHashCommands.SetIfNotExists(string key, string field, byte[] value, object asyncState)
        {
            return this.ExecuteIntegerReply(new RedisRequest(4).AddArgument("HSETNX")
                .AddArgument(key).AddArgument(field).AddArgument(value), asyncState);
        }
        #endregion

        #region Pub/Sub Members
        /// <summary>
        /// Posts a message to the given channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        public Task<int> Publish(string channel, string message, object asyncState = null)
        {
            return this.ExecuteIntegerReply(new RedisRequest(3).AddArgument("PUBLISH").AddArgument(channel).AddArgument(message), asyncState);
        }
        /// <summary>
        /// Posts a message to the given channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        public Task<int> Publish(string channel, byte[] message, object asyncState = null)
        {
            return this.ExecuteIntegerReply(new RedisRequest(3).AddArgument("PUBLISH").AddArgument(channel).AddArgument(message), asyncState);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// execute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload"></param>
        /// <param name="callback"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">callback is null.</exception>
        private Task<T> Execute<T>(byte[] payload, Action<TaskCompletionSource<T>, RedisResponse> callback, object asyncState)
        {
            if (callback == null) throw new ArgumentNullException("callback");

            var source = new TaskCompletionSource<T>(asyncState);
            base.Send(new Request<RedisResponse>(base.NextRequestSeqID(), string.Empty, payload,
                ex => source.TrySetException(ex), response => callback(source, response)));
            return source.Task;
        }
        /// <summary>
        /// ExecuteIntegerReply
        /// </summary>
        /// <param name="request"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        private Task<int> ExecuteIntegerReply(RedisRequest request, object asyncState)
        {
            return this.Execute<int>(request.ToPayload(), (source, response) =>
            {
                var intReply = response.Reply as IntegerReply;
                if (intReply != null)
                {
                    source.TrySetResult(intReply.Value);
                    return;
                }
                if (response.Reply is ErrorReply)
                {
                    source.TrySetException((response.Reply as ErrorReply).Error());
                    return;
                }
                source.TrySetException(new RedisException("Failed to resolve the Reply"));
            }, asyncState);
        }
        /// <summary>
        /// ExecuteIntegerReply2
        /// </summary>
        /// <param name="request"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        private Task<bool> ExecuteIntegerReply2(RedisRequest request, object asyncState)
        {
            return this.Execute<bool>(request.ToPayload(), (source, response) =>
            {
                var intReply = response.Reply as IntegerReply;
                if (intReply != null)
                {
                    source.TrySetResult(intReply.Value == 1);
                    return;
                }
                if (response.Reply is ErrorReply)
                {
                    source.TrySetException((response.Reply as ErrorReply).Error());
                    return;
                }
                source.TrySetException(new RedisException("Failed to resolve the Reply"));
            }, asyncState);
        }
        /// <summary>
        /// ExecuteStatusReply
        /// </summary>
        /// <param name="request"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        private Task<string> ExecuteStatusReply(RedisRequest request, object asyncState)
        {
            return this.Execute<string>(request.ToPayload(), (source, response) =>
            {
                var statusReply = response.Reply as StatusReply;
                if (statusReply != null)
                {
                    source.TrySetResult(statusReply.Status);
                    return;
                }
                if (response.Reply is ErrorReply)
                {
                    source.TrySetException((response.Reply as ErrorReply).Error());
                    return;
                }
                source.TrySetException(new RedisException("Failed to resolve the Reply"));
            }, asyncState);
        }
        /// <summary>
        /// ExecuteBulkReplies
        /// </summary>
        /// <param name="request"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        private Task<byte[]> ExecuteBulkReplies(RedisRequest request, object asyncState)
        {
            return this.Execute<byte[]>(request.ToPayload(), (source, response) =>
            {
                var mbReeply = response.Reply as BulkReplies;
                if (mbReeply != null)
                {
                    source.TrySetResult(mbReeply.Payload);
                    return;
                }
                if (response.Reply is ErrorReply)
                {
                    source.TrySetException((response.Reply as ErrorReply).Error());
                    return;
                }
                source.TrySetException(new RedisException("Failed to resolve the Reply"));
            }, asyncState);
        }
        /// <summary>
        /// ExecuteBulkReplies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="valueFactory"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">valueFactory is null.</exception>
        private Task<T> ExecuteBulkReplies<T>(RedisRequest request, Func<byte[], T> valueFactory, object asyncState)
        {
            if (valueFactory == null) throw new ArgumentNullException("valueFactory");

            return this.Execute<T>(request.ToPayload(), (source, response) =>
            {
                var mbReeply = response.Reply as BulkReplies;
                if (mbReeply != null)
                {
                    if (mbReeply.Payload == null || mbReeply.Payload.Length == 0)
                    {
                        source.TrySetResult(default(T));
                        return;
                    }

                    try { source.TrySetResult(valueFactory(mbReeply.Payload)); }
                    catch (Exception ex)
                    {
                        source.TrySetException(ex);
                    }
                    return;
                }

                if (response.Reply is ErrorReply)
                {
                    source.TrySetException((response.Reply as ErrorReply).Error());
                    return;
                }

                source.TrySetException(new RedisException("Failed to resolve the Reply"));
            }, asyncState);
        }
        /// <summary>
        /// ExecuteMultiBulkReplies
        /// </summary>
        /// <param name="request"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        private Task<byte[][]> ExecuteMultiBulkReplies(RedisRequest request, object asyncState)
        {
            return this.Execute<byte[][]>(request.ToPayload(), (source, response) =>
            {
                var mbReeply = response.Reply as MultiBulkReplies;
                if (mbReeply != null)
                {
                    if (mbReeply.Replies == null)
                    {
                        source.TrySetResult(null);
                        return;
                    }

                    byte[][] arrBytes = new byte[mbReeply.Replies.Length][];
                    for (int i = 0, l = mbReeply.Replies.Length; i < l; i++)
                    {
                        var objBulk = mbReeply.Replies[i] as BulkReplies;
                        if (objBulk != null) arrBytes[i] = objBulk.Payload;
                    }
                    source.TrySetResult(arrBytes);
                    return;
                }
                if (response.Reply is ErrorReply)
                {
                    source.TrySetException((response.Reply as ErrorReply).Error());
                    return;
                }
                source.TrySetException(new RedisException("Failed to resolve the Reply"));
            }, asyncState);
        }
        /// <summary>
        /// ExecuteMultiBulkReplies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="valueFactory"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">valueFactory is null.</exception>
        private Task<T[]> ExecuteMultiBulkReplies<T>(RedisRequest request, Func<byte[], T> valueFactory, object asyncState)
        {
            if (valueFactory == null) throw new ArgumentNullException("valueFactory");

            return this.Execute<T[]>(request.ToPayload(), (source, response) =>
            {
                var mbReeply = response.Reply as MultiBulkReplies;
                if (mbReeply != null)
                {
                    if (mbReeply.Replies == null)
                    {
                        source.TrySetResult(null);
                        return;
                    }

                    var arrResults = new T[mbReeply.Replies.Length];
                    for (int i = 0, l = mbReeply.Replies.Length; i < l; i++)
                    {
                        var objBulk = mbReeply.Replies[i] as BulkReplies;
                        if (objBulk == null) continue;

                        if (objBulk.Payload == null || objBulk.Payload.Length == 0)
                        {
                            arrResults[i] = default(T);
                            continue;
                        }

                        try { arrResults[i] = valueFactory(objBulk.Payload); }
                        catch (Exception ex)
                        {
                            source.TrySetException(ex);
                            return;
                        }
                    }
                    source.TrySetResult(arrResults);
                    return;
                }
                if (response.Reply is ErrorReply)
                {
                    source.TrySetException((response.Reply as ErrorReply).Error());
                    return;
                }
                source.TrySetException(new RedisException("Failed to resolve the Reply"));
            }, asyncState);
        }
        #endregion
    }
}
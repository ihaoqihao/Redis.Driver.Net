using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Sodao.FastSocket.Client;

namespace Redis.Driver
{
    /// <summary>
    /// redis receiveing queue
    /// </summary>
    public sealed class RedisReceivingQueue : IRequestReceivingCollection<IRedisReply>
    {
        #region Private Members
        private readonly int _millisecondsReceiveTimeout;
        /// <summary>
        /// key:connectionID
        /// </summary>
        private readonly ConcurrentDictionary<long, ItemQueue> _dic = new ConcurrentDictionary<long, ItemQueue>();
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="millisecondsReceiveTimeout">接收超时时间</param>
        public RedisReceivingQueue(int millisecondsReceiveTimeout)
        {
            this._millisecondsReceiveTimeout = millisecondsReceiveTimeout;
        }
        #endregion

        #region IRequestReceivingCollection Members
        /// <summary>
        /// 入列
        /// </summary>
        /// <param name="request"></param>
        public void Enqueue(Request<IRedisReply> request)
        {
            this._dic.GetOrAdd(request.ConnectionIDBySent, id => new ItemQueue()).Enqueue(request);
        }
        /// <summary>
        /// 清除指定连接ID下, response匹配的请求并返回
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public Request<IRedisReply> Remove(long connectionID, IRedisReply response)
        {
            ItemQueue item = null;
            if (this._dic.TryGetValue(connectionID, out item))
                return item.Dequeue();

            return null;
        }
        /// <summary>
        /// 清除指定连接ID下的所有请求并返回
        /// </summary>
        /// <param name="connectionID"></param>
        /// <returns></returns>
        public Request<IRedisReply>[] Clear(long connectionID)
        {
            ItemQueue item = null;
            if (this._dic.TryRemove(connectionID, out item))
            {
                item.Close();
                return item.DequeueAll();
            }
            return null;
        }
        #endregion

        #region Child Class
        /// <summary>
        /// item queue
        /// </summary>
        private class ItemQueue
        {
            #region Private Members
            private readonly Queue<Request<IRedisReply>> _queue = new Queue<Request<IRedisReply>>();
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            public ItemQueue()
            {
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// 入列
            /// </summary>
            /// <param name="request"></param>
            /// <exception cref="ArgumentNullException">request is null</exception>
            public void Enqueue(Request<IRedisReply> request)
            {
                if (request == null)
                    throw new ArgumentNullException("request");

                lock (this)
                    this._queue.Enqueue(request);
            }
            /// <summary>
            /// 出列
            /// </summary>
            /// <returns></returns>
            public Request<IRedisReply> Dequeue()
            {
                lock (this)
                {
                    if (this._queue.Count == 0)
                        return null;

                    return this._queue.Dequeue();
                }
            }
            /// <summary>
            /// 全部出列
            /// </summary>
            /// <returns></returns>
            public Request<IRedisReply>[] DequeueAll()
            {
                lock (this)
                {
                    if (this._queue.Count == 0)
                        return null;

                    var arr = this._queue.ToArray();
                    this._queue.Clear();
                    return arr;
                }
            }
            /// <summary>
            /// close
            /// </summary>
            public void Close()
            {
            }
            #endregion
        }
        #endregion
    }
}
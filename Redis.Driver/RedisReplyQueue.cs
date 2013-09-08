using System.Collections.Generic;

namespace Redis.Driver
{
    /// <summary>
    /// redis reply queue
    /// </summary>
    public sealed class RedisReplyQueue 
    {
        private readonly Queue<int> _innerQueue = new Queue<int>();

        /// <summary>
        /// enqueue
        /// </summary>
        /// <param name="seqID"></param>
        public void Enqueue(int seqID)
        {
            lock (this) this._innerQueue.Enqueue(seqID);
        }
        /// <summary>
        /// dequeue
        /// </summary>
        /// <returns></returns>
        public int Dequeue()
        {
            lock (this)
            {
                if (this._innerQueue.Count > 0) return this._innerQueue.Dequeue();
            }
            return -1;
        }
    }
}
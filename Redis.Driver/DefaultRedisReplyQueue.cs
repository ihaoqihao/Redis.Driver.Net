using System.Collections.Generic;

namespace Redis.Driver
{
    /// <summary>
    /// default redis reply queue
    /// </summary>
    public sealed class DefaultRedisReplyQueue : IRedisReplyQueue
    {
        #region Private Members
        private readonly Queue<int> _innerQueue = new Queue<int>();
        #endregion

        #region IRedisReplyList Members
        /// <summary>
        /// enqueue
        /// </summary>
        /// <param name="seqID"></param>
        public void Enqueue(int seqID)
        {
            System.Console.WriteLine("+" + seqID.ToString());
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
                if (this._innerQueue.Count > 0)
                {
                    var seqID = this._innerQueue.Dequeue();
                    System.Console.WriteLine("-" + seqID.ToString());
                    return seqID;
                }
            }
            return -1;
        }
        #endregion
    }
}
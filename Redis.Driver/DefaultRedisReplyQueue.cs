using System.Collections.Generic;

namespace Redis.Driver
{
    /// <summary>
    /// default redis reply queue
    /// </summary>
    public sealed class DefaultRedisReplyQueue : IRedisReplyQueue
    {
        #region Private Members
        private readonly LinkedList<int> _list = new LinkedList<int>();
        #endregion

        #region IRedisReplyList Members
        /// <summary>
        /// Adds seqID to the end of the list.
        /// </summary>
        /// <param name="seqID"></param>
        public void Enqueue(int seqID)
        {
            lock (this)
                this._list.AddLast(seqID);
        }
        /// <summary>
        /// Removes and returns the seqID at the end of the list.
        /// </summary>
        /// <returns></returns>
        public int Unenqueue()
        {
            lock (this)
                if (this._list.Count > 0)
                {
                    var node = this._list.Last;
                    this._list.RemoveLast();
                    return node.Value;
                }

            return -1;
        }
        /// <summary>
        /// Removes and returns the seqID at the beginning of the list.
        /// </summary>
        /// <returns></returns>
        public int Dequeue()
        {
            lock (this)
                if (this._list.Count > 0)
                {
                    var node = this._list.First;
                    this._list.RemoveFirst();
                    return node.Value;
                }

            return -1;
        }
        #endregion
    }
}
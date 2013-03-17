
namespace Redis.Driver
{
    /// <summary>
    /// a redis reply queue interface
    /// </summary>
    public interface IRedisReplyQueue
    {
        /// <summary>
        /// Adds seqID to the end of the list.
        /// </summary>
        /// <param name="seqID"></param>
        void Enqueue(int seqID);
        /// <summary>
        /// Removes and returns the seqID at the end of the list.
        /// </summary>
        /// <returns></returns>
        int Unenqueue();
        /// <summary>
        /// Removes and returns the seqID at the beginning of the list.
        /// </summary>
        /// <returns></returns>
        int Dequeue();
    }
}
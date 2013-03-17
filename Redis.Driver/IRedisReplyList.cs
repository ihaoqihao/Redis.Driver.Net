
namespace Redis.Driver
{
    /// <summary>
    /// a redis reply list interface
    /// </summary>
    public interface IRedisReplyList
    {
        /// <summary>
        /// Adds seqID to the end of the list.
        /// </summary>
        /// <param name="seqID"></param>
        void Enqueue(int seqID);
        /// <summary>
        /// Removes and returns the seqID at the beginning of the list.
        /// </summary>
        /// <returns></returns>
        int Dequeue();
        /// <summary>
        /// Removes and returns the seqID at the end of the list.
        /// </summary>
        /// <returns></returns>
        int Pull();
    }
}

namespace Redis.Driver
{
    /// <summary>
    /// a redis reply queue interface
    /// </summary>
    public interface IRedisReplyQueue
    {
        /// <summary>
        /// enqueue
        /// </summary>
        /// <param name="seqID"></param>
        void Enqueue(int seqID);
        /// <summary>
        /// un enqueue
        /// </summary>
        /// <returns></returns>
        int Unenqueue();
        /// <summary>
        /// dequeue
        /// </summary>
        /// <returns></returns>
        int Dequeue();
    }
}
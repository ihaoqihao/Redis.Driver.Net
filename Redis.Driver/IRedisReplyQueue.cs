
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
        /// dequeue
        /// </summary>
        /// <returns></returns>
        int Dequeue();
    }
}
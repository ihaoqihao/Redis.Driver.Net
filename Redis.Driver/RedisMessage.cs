using System;

namespace Redis.Driver
{
    /// <summary>
    /// redis message
    /// </summary>
    public sealed class RedisMessage : Sodao.FastSocket.Client.Messaging.IMessage
    {
        /// <summary>
        /// seqId
        /// </summary>
        private readonly int _seqId;
        /// <summary>
        /// reply
        /// </summary>
        public readonly IRedisReply Reply;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqId"></param>
        /// <param name="reply"></param>
        /// <exception cref="ArgumentNullException">reply is null.</exception>
        public RedisMessage(int seqId, IRedisReply reply)
        {
            if (reply == null) throw new ArgumentNullException("reply");
            this._seqId = seqId;
            this.Reply = reply;
        }

        /// <summary>
        /// seqId
        /// </summary>
        public int SeqId
        {
            get { return this._seqId; }
        }
    }
}
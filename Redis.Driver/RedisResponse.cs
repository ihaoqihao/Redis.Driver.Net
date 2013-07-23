using System;

namespace Redis.Driver
{
    /// <summary>
    /// redis response
    /// </summary>
    public sealed class RedisResponse : Sodao.FastSocket.Client.Response.IResponse
    {
        private int _seqID;
        /// <summary>
        /// reply
        /// </summary>
        public readonly IRedisReply Reply;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqID"></param>
        /// <param name="reply"></param>
        /// <exception cref="ArgumentNullException">reply is null.</exception>
        public RedisResponse(int seqID, IRedisReply reply)
        {
            if (reply == null) throw new ArgumentNullException("reply");

            this._seqID = seqID;
            this.Reply = reply;
        }

        /// <summary>
        /// get seqID
        /// </summary>
        public int SeqID
        {
            get { return this._seqID; }
        }
    }
}
using System;
using Sodao.FastSocket.Client.Response;

namespace Redis.Driver
{
    /// <summary>
    /// redis reply interface.
    /// </summary>
    public interface IRedisReply : IResponse
    {
    }

    #region BaseReply
    /// <summary>
    /// base redis reply
    /// </summary>
    public abstract class BaseReply : IRedisReply
    {
        private int _seqID;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqID"></param>
        public BaseReply(int seqID)
        {
            this._seqID = seqID;
        }

        /// <summary>
        /// get seqID
        /// </summary>
        public int SeqID
        {
            get { return this._seqID; }
        }
    }
    #endregion

    #region Error reply
    /// <summary>
    /// error reply
    /// </summary>
    public sealed class ErrorReply : BaseReply
    {
        #region Public Members
        /// <summary>
        /// error message
        /// </summary>
        public readonly string ErrorMessage;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqID"></param>
        /// <param name="errorMessage"></param>
        /// <exception cref="ArgumentNullException">errorMessage is null or empty.</exception>
        public ErrorReply(int seqID, string errorMessage)
            : base(seqID)
        {
            if (string.IsNullOrEmpty(errorMessage))
                throw new ArgumentNullException("errorMessage");

            this.ErrorMessage = errorMessage;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// return RedisException
        /// </summary>
        /// <returns></returns>
        public RedisException Error()
        {
            return new RedisException(this.ErrorMessage);
        }
        #endregion
    }
    #endregion

    #region Integer reply
    /// <summary>
    /// Iinteger reply
    /// </summary>
    public sealed class IntegerReply : BaseReply
    {
        #region Public Members
        /// <summary>
        /// value
        /// </summary>
        public readonly int Value;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqID"></param>
        /// <param name="value"></param>
        public IntegerReply(int seqID, int value)
            : base(seqID)
        {
            this.Value = value;
        }
        #endregion
    }
    #endregion

    #region Status reply
    /// <summary>
    /// status reply
    /// </summary>
    public sealed class StatusReply : BaseReply
    {
        #region Public Members
        /// <summary>
        /// status
        /// </summary>
        public readonly string Status;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqID"></param>
        /// <param name="status"></param>
        /// <exception cref="ArgumentNullException">status is null or empty.</exception>
        public StatusReply(int seqID, string status)
            : base(seqID)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentNullException("status");

            this.Status = status;
        }
        #endregion
    }
    #endregion

    #region Bulk replies
    /// <summary>
    /// bulk replies
    /// </summary>
    public sealed class BulkReplies : BaseReply
    {
        #region Public Members
        /// <summary>
        /// payload
        /// </summary>
        public readonly byte[] Payload;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqID"></param>
        /// <param name="payload"></param>
        public BulkReplies(int seqID, byte[] payload)
            : base(seqID)
        {
            this.Payload = payload;
        }
        #endregion
    }
    #endregion

    #region Multi-bulk replies
    /// <summary>
    /// Multi-bulk replies
    /// </summary>
    public sealed class MultiBulkReplies : BaseReply
    {
        #region Public Members
        /// <summary>
        /// payloads
        /// </summary>
        public readonly byte[][] Payloads;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqID"></param>
        /// <param name="payloads"></param>
        public MultiBulkReplies(int seqID, byte[][] payloads)
            : base(seqID)
        {
            this.Payloads = payloads;
        }
        #endregion
    }
    #endregion
}
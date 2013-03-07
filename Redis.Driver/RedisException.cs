using System;

namespace Redis.Driver
{
    /// <summary>
    /// redis exception
    /// </summary>
    public sealed class RedisException : ApplicationException
    {
        /// <summary>
        /// new
        /// </summary>
        /// <param name="message"></param>
        public RedisException(string message)
            : base(message)
        {
        }
    }
}
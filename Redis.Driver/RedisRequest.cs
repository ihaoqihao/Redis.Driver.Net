using System;
using System.IO;
using System.Text;

namespace Redis.Driver
{
    /// <summary>
    /// redis request
    /// </summary>
    public sealed class RedisRequest
    {
        #region Private Members
        private MemoryStream _stream = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="argNumber">参数数量</param>
        /// <exception cref="ArgumentNullException">argNumber less than 1.</exception>
        public RedisRequest(int argNumber)
        {
            if (argNumber < 1)
                throw new ArgumentNullException("argNumber", "argNumber less than 1.");

            this._stream = new MemoryStream();
            this._stream.WriteByte((byte)'*');
            Write(this._stream, argNumber);
            this._stream.WriteByte(13);
            this._stream.WriteByte(10);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// add argument
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">value is null or empty.</exception>
        public RedisRequest AddArgument(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("value");

            var bytes = Encoding.UTF8.GetBytes(value);
            Write(this._stream, bytes.Length);
            this._stream.WriteByte(13);
            this._stream.WriteByte(10);
            this._stream.Write(bytes, 0, bytes.Length);
            this._stream.WriteByte(13);
            this._stream.WriteByte(10);

            return this;
        }
        /// <summary>
        /// add argument
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        public RedisRequest AddArgument(byte[] value)
        {
            if (value == null || value.Length == 0)
                throw new ArgumentNullException("value");

            Write(this._stream, value.Length);
            this._stream.WriteByte(13);
            this._stream.WriteByte(10);
            this._stream.Write(value, 0, value.Length);
            this._stream.WriteByte(13);
            this._stream.WriteByte(10);

            return this;
        }
        /// <summary>
        /// to payload
        /// </summary>
        /// <returns></returns>
        public byte[] ToPayload()
        {
            if (this._stream == null)
                return null;

            using (this._stream)
            {
                this._stream = null;
                return this._stream.ToArray();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// write interger
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        static private void Write(MemoryStream stream, int value)
        {
            if (value >= 0 && value <= 9)
                stream.WriteByte((byte)((int)'0' + (int)value));
            else if (value < 0 && value >= -9)
            {
                stream.WriteByte((byte)'-');
                stream.WriteByte((byte)((int)'0' - (int)value));
            }
            else
            {
                var bytes = Encoding.ASCII.GetBytes(value.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        #endregion
    }
}
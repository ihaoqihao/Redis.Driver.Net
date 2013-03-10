using System.Threading.Tasks;

namespace Redis.Driver
{
    /// <summary>
    /// redis.strings命令
    /// </summary>
    /// <remarks>http://redis.io/commands#string</remarks>
    public interface IStringCommands
    {
        /// <summary>
        /// If key already exists and is a string, 
        /// this command appends the value at the end of the string. 
        /// If key does not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>the length of the string after the append operation.</returns>
        Task<int> Append(string key, string value);
        /// <summary>
        /// If key already exists and is a string, 
        /// this command appends the value at the end of the string. 
        /// If key does not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>the length of the string after the append operation.</returns>
        Task<int> Append(string key, byte[] value);
        /// <summary>
        /// When offset is beyond the string length, 
        /// the string is assumed to be a contiguous space with 0 bits. When key does not exist it is assumed to be an empty string, 
        /// so offset is always out of range and the value is also assumed to be a contiguous space with 0 bits.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <returns>the bit value stored at offset.</returns>
        Task<int> GetBit(string key, int offset);
        /// <summary>
        /// Returns the values of all specified keys. 
        /// For every key that does not hold a string value or does not exist, 
        /// the special value nil is returned. Because of this, the operation never fails.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>list of values at the specified keys.</returns>
        Task<byte[][]> Get(params string[] keys);
        /// <summary>
        /// Get the value of key. 
        /// If the key does not exist the special value nil is returned. 
        /// An error is returned if the value stored at key is not a string, because GET only handles string values.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>the value of key, or nil when key does not exist.</returns>
        Task<byte[]> Get(string key);
    }
}
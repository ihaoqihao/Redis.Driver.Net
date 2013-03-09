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
        /// <remarks>http://redis.io/commands/append</remarks>
        Task<int> Append(string key, string value);
        /// <summary>
        /// If key already exists and is a string, 
        /// this command appends the value at the end of the string. 
        /// If key does not exist it is created and set as an empty string, so APPEND will be similar to SET in this special case.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>the length of the string after the append operation.</returns>
        /// <remarks>http://redis.io/commands/append</remarks>
        Task<int> Append(string key, byte[] value);
    }
}
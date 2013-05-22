using System.Threading.Tasks;

namespace Redis.Driver
{
    /// <summary>
    /// redis.keys命令
    /// </summary>
    /// <remarks>http://redis.io/commands#generic</remarks>
    public interface IKeyCommands
    {
        /// <summary>
        /// Removes the specified keys. A key is ignored if it does not exist.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="asyncState"></param>
        /// <returns>The number of keys that were removed.</returns>
        Task<int> Del(string key, object asyncState = null);
        /// <summary>
        /// Removes the specified keys. A key is ignored if it does not exist.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="asyncState"></param>
        /// <returns>The number of keys that were removed.</returns>
        Task<int> Del(string[] keys, object asyncState = null);
        /// <summary>
        /// Returns all keys matching pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="asyncState"></param>
        /// <returns>list of keys matching pattern.</returns>
        Task<string[]> Keys(string pattern, object asyncState = null);
        /// <summary>
        /// Set a timeout on key. After the timeout has expired, the key will automatically be deleted. 
        /// A key with an associated timeout is often said to be volatile in Redis terminology.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        Task Expire(string key, int seconds, object asyncState = null);
    }
}
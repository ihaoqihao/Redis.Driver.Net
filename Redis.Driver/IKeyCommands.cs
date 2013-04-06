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
        /// This command works exactly like EXPIRE but the time to live of the key is specified in milliseconds instead of seconds.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// 1 if the timeout was set.
        /// 0 if key does not exist or the timeout could not be set.
        /// </returns>
        Task<int> PExpire(string key, object asyncState = null);
        /// <summary>
        /// Renames key to newkey if newkey does not yet exist. 
        /// It returns an error under the same conditions as RENAME.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newKey"></param>
        /// <param name="asyncState"></param>
        /// <returns>
        /// 1 if key was renamed to newkey.
        /// 0 if newkey already exists.
        /// </returns>
        Task<int> RenameNX(string key, string newKey, object asyncState = null);
    }
}
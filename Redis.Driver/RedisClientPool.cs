using System;
using System.Collections.Concurrent;

namespace Redis.Driver
{
    /// <summary>
    /// redis client pool
    /// </summary>
    static public class RedisClientPool
    {
        /// <summary>
        /// key:string.Concat(configFile, endpointName)
        /// </summary>
        static private readonly ConcurrentDictionary<string, Lazy<RedisClient>> _dic =
            new ConcurrentDictionary<string, Lazy<RedisClient>>();

        /// <summary>
        /// get <see cref="RedisClient"/>
        /// </summary>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        static public RedisClient Get(string endpointName)
        {
            return Get(null, endpointName);
        }
        /// <summary>
        /// get <see cref="RedisClient"/>
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">endpointName is null or empty.</exception>
        static public RedisClient Get(string configFile, string endpointName)
        {
            if (string.IsNullOrEmpty(endpointName)) throw new ArgumentNullException("endpointName");
            if (configFile == null) configFile = string.Empty;

            return _dic.GetOrAdd(string.Concat(configFile, endpointName),
                key => new Lazy<RedisClient>(() => RedisClientFactory.Create(configFile, endpointName), true)).Value;
        }
    }
}
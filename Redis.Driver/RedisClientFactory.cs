using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Net;

namespace Redis.Driver
{
    /// <summary>
    /// redis client factory
    /// </summary>
    static public class RedisClientFactory
    {
        static private readonly ConcurrentDictionary<string, Lazy<RedisClient>> _dic =
            new ConcurrentDictionary<string, Lazy<RedisClient>>();

        /// <summary>
        /// get <see cref="RedisClient"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">endpointName is null or empty.</exception>
        static public RedisClient Get(string endpointName)
        {
            if (string.IsNullOrEmpty(endpointName))
                throw new ArgumentNullException("endpointName");

            return _dic.GetOrAdd(endpointName, key => new Lazy<RedisClient>(() =>
            {
                //get config
                var config = ConfigurationManager.GetSection("redis") as Config.RedisConfigSection;
                var clientConfig = config.Clients.Get(key);

                var redisClient = new RedisClient(clientConfig.SocketBufferSize,
                    clientConfig.MessageBufferSize,
                    clientConfig.MillisecondsSendTimeout,
                    clientConfig.MillisecondsReceiveTimeout);

                foreach (Config.ServerConfig server in clientConfig.Servers)
                    redisClient.RegisterServerNode(string.Concat(server.Host, server.Port),
                        new IPEndPoint(IPAddress.Parse(server.Host), server.Port));
                
                return redisClient;
            }, true)).Value;
        }
    }
}
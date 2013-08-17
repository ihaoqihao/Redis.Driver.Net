using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Net;

namespace Redis.Driver
{
    /// <summary>
    /// redis client factory
    /// </summary>
    static public class RedisClientFactory
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

            return _dic.GetOrAdd(string.Concat(configFile, endpointName), key => new Lazy<RedisClient>(() =>
            {
                Config.RedisConfigSection config = null;

                if (string.IsNullOrEmpty(configFile)) config = ConfigurationManager.GetSection("redis") as Config.RedisConfigSection;
                else
                {
                    config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
                    {
                        ExeConfigFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile)
                    }, ConfigurationUserLevel.None).GetSection("redis") as Config.RedisConfigSection;
                }

                var clientConfig = config.Clients.Get(endpointName);

                var redisClient = new RedisClient(clientConfig.SocketBufferSize,
                    clientConfig.MessageBufferSize,
                    clientConfig.MillisecondsSendTimeout,
                    clientConfig.MillisecondsReceiveTimeout);

                foreach (Config.ServerConfig server in clientConfig.Servers)
                    redisClient.RegisterServerNode(string.Concat(server.Host, server.Port), new IPEndPoint(IPAddress.Parse(server.Host), server.Port));

                return redisClient;
            }, true)).Value;
        }
    }
}
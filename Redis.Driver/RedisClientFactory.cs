using System;
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
        /// create <see cref="RedisClient"/>
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">endpointName is null or empty.</exception>
        static public RedisClient Create(string configFile, string endpointName)
        {
            if (string.IsNullOrEmpty(endpointName)) throw new ArgumentNullException("endpointName");

            Config.RedisConfigSection config = null;
            if (string.IsNullOrEmpty(configFile)) config = ConfigurationManager.GetSection("redis") as Config.RedisConfigSection;
            else
            {
                config = ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap { ExeConfigFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile) },
                    ConfigurationUserLevel.None).GetSection("redis") as Config.RedisConfigSection;
            }

            var clientConfig = config.Clients.Get(endpointName);

            var redisClient = new RedisClient(clientConfig.SocketBufferSize,
                clientConfig.MessageBufferSize,
                clientConfig.MillisecondsSendTimeout,
                clientConfig.MillisecondsReceiveTimeout);

            foreach (Config.ServerConfig server in clientConfig.Servers)
                redisClient.TryRegisterEndPoint(string.Concat(server.Host, server.Port), new IPEndPoint(IPAddress.Parse(server.Host), server.Port));

            return redisClient;
        }
    }
}
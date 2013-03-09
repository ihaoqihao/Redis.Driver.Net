using System.Configuration;

namespace Redis.Driver.Config
{
    /// <summary>
    /// redis config section
    /// </summary>
    public class RedisConfigSection : ConfigurationSection
    {
        /// <summary>
        /// endpoint collection。
        /// </summary>
        [ConfigurationProperty("client", IsRequired = true)]
        public EndpointCollection Clients
        {
            get { return this["client"] as EndpointCollection; }
        }
    }
}
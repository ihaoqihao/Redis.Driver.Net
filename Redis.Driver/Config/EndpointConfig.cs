using System;
using System.Configuration;

namespace Redis.Driver.Config
{
    /// <summary>
    /// endpoint config
    /// </summary>
    public class EndpointConfig : ConfigurationElement
    {
        /// <summary>
        /// name
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (String)this["name"]; }
        }
        /// <summary>
        /// socket buffer size
        /// default is 1024 byte
        /// </summary>
        [ConfigurationProperty("socketBufferSize", IsRequired = false, DefaultValue = 1024)]
        public int SocketBufferSize
        {
            get { return (int)this["socketBufferSize"]; }
        }
        /// <summary>
        /// message buffer size
        /// default is 1024 byte
        /// </summary>
        [ConfigurationProperty("messageBufferSize", IsRequired = false, DefaultValue = 1024)]
        public int MessageBufferSize
        {
            get { return (int)this["messageBufferSize"]; }
        }
        /// <summary>
        /// 发送超时值，毫秒单位
        /// </summary>
        [ConfigurationProperty("millisecondsSendTimeout", IsRequired = false, DefaultValue = 5000)]
        public int MillisecondsSendTimeout
        {
            get { return (int)(this["millisecondsSendTimeout"]); }
        }
        /// <summary>
        /// 接收超时值，毫秒单位
        /// </summary>
        [ConfigurationProperty("millisecondsReceiveTimeout", IsRequired = false, DefaultValue = 5000)]
        public int MillisecondsReceiveTimeout
        {
            get { return (int)(this["millisecondsReceiveTimeout"]); }
        }
        /// <summary>
        /// 服务器集合。
        /// </summary>
        [ConfigurationProperty("servers", IsRequired = true)]
        public ServerCollection Servers
        {
            get { return this["servers"] as ServerCollection; }
        }
    }
}
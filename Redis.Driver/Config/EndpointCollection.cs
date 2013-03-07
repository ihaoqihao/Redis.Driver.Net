using System.Configuration;

namespace Redis.Driver.Config
{
    /// <summary>
    /// endpoint collection 
    /// </summary>
    [ConfigurationCollection(typeof(EndpointConfig), AddItemName = "endpoint")]
    public class EndpointCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// create new element
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new EndpointConfig();
        }
        /// <summary>
        /// 获取指定元素的Key。
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as EndpointConfig).Name;
        }
        /// <summary>
        /// 获取指定位置的对象。
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public EndpointConfig this[int i]
        {
            get { return BaseGet(i) as EndpointConfig; }
        }
        /// <summary>
        /// 获取指定key的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public EndpointConfig Get(string key)
        {
            return BaseGet(key) as EndpointConfig;
        }
    }
}
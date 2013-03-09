using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Keys
    {
        [Test]
        public void Append()
        {
            var client = Redis.Driver.RedisClientFactory.Get("test1");
            client.Strings.Append("mykey1", "fuck");
        }
    }
}
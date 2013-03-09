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
            var result = client.Strings.Append("mykey1", "fuck").Result;
            Assert.Greater(result, 0);
        }
    }
}
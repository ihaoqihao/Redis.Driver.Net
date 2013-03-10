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
        [Test]
        public void GetBit()
        {
            var client = Redis.Driver.RedisClientFactory.Get("test1");
            client.Strings.GetBit("key1", 7).ContinueWith(c =>
            {
                Console.WriteLine(c.Result);
            });
        }
        [Test]
        public void Mget()
        {
            var client = Redis.Driver.RedisClientFactory.Get("test1");
            client.Strings.Get("a", "b", "c").Wait();
        }
        [Test]
        public void Get()
        {
            var client = Redis.Driver.RedisClientFactory.Get("test1");
            client.Strings.Get("a").Wait();
        }
    }
}
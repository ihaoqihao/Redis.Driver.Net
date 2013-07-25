using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Hash
    {
        [Test]
        public void Set()
        {
            var client = Redis.Driver.RedisClientFactory.Get("test1");
            var r = client.Hashes.Set("key1", "field1", "value1").Result;
            var r2 = client.Hashes.Set("key1", "field2", BitConverter.GetBytes(DateTime.Now.Ticks)).Result;

            var dic = new Dictionary<string, byte[]>();
            dic.Add("field3", BitConverter.GetBytes(DateTime.Now.Ticks));
            dic.Add("field4", BitConverter.GetBytes(DateTime.Now.Ticks));
            dic.Add("field5", BitConverter.GetBytes(DateTime.Now.Ticks));
            client.Hashes.Set("key1", dic).Wait();
        }
        [Test]
        public void Get()
        {
            var client = Redis.Driver.RedisClientFactory.Get("test1");
            Assert.IsTrue(Encoding.UTF8.GetString(client.Hashes.Get("key1", "field1").Result) == "value1");
        }
        [Test]
        public void GetAll()
        {
            var client = Redis.Driver.RedisClientFactory.Get("test1");
            var dic = client.Hashes.GetAll("key12").Result;
            foreach (var child in dic)
            {
                Console.WriteLine(child.Key);
                Console.WriteLine(BitConverter.ToString(child.Value));
            }
        }

        [Test]
        public void Remove()
        {
            var client = Redis.Driver.RedisClientFactory.Get("test1");
            client.Hashes.Remove("key1", "field1").Wait();
            client.Hashes.Remove("key1", new string[] { "field1", "field3" }).Wait();
        }
    }
}
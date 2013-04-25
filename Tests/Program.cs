using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    static public class Program
    {
        static public void Main()
        {
            var sub = new Redis.Driver.RedisSubscriber("redis://127.0.0.1:6379");
            sub.Listener += (channel, payload) =>
            {
                Console.WriteLine(channel);
                Console.WriteLine(Encoding.UTF8.GetString(payload));
            };
            sub.Subscribe("aaa");

            Redis.Driver.RedisClientFactory.Get("test1").Publish("aaa", "asdfasdf");

            Console.ReadLine();
        }
    }
}
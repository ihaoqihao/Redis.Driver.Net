using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Sodao.FastSocket.SocketBase.Log.Trace.EnableConsole();

            var client = Redis.Driver.RedisClientFactory.Get("test1");

            //set key1 value1
            client.Strings.Set("key1", "value1").ContinueWith(c => Console.WriteLine(c.Exception.ToString()),
                System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

            client.Strings.Set("key2", "value2").ContinueWith(c => Console.WriteLine(c.Exception.ToString()),
                System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

            ////get key1 value
            client.Strings.Get("key1").ContinueWith(c =>
            {
                if (c.IsFaulted) Console.WriteLine(c.Exception.ToString());
                else Console.WriteLine(Encoding.UTF8.GetString(c.Result));
            });

            client.Strings.Get("key2").ContinueWith(c =>
            {
                if (c.IsFaulted) Console.WriteLine(c.Exception.ToString());
                else Console.WriteLine(Encoding.UTF8.GetString(c.Result));
            });

            //redis subscriber
            var sub = new Redis.Driver.RedisSubscriber("127.0.0.1", 6379);
            sub.PatternSubscribe("fuck*123");
            sub.Listener += (channel, payload) =>
            {
                Console.WriteLine(channel);
                Console.WriteLine(Encoding.UTF8.GetString(payload));
            };

            Console.ReadLine();
        }
    }
}
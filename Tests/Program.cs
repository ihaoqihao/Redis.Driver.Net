using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    class Program
    {
        static private Redis.Driver.RedisClient redis =
            Redis.Driver.RedisClientPool.Get("redis.config", "test");

        static public void Main()
        {
            System.Threading.ThreadPool.SetMinThreads(30, 30);
            Sodao.FastSocket.SocketBase.Log.Trace.EnableConsole();

            var subscriber = new Redis.Driver.RedisSubscriber("127.0.0.1", 6379);
            subscriber.Listener += (channel, message) =>
            {
                Console.WriteLine("channel:" + channel + " message:" + Encoding.UTF8.GetString(message));
            };
            subscriber.Subscribe("channel1");

            Enumerable.Range(10, 50).ToList().ForEach(i =>
            {
                int index = i;
                System.Threading.Tasks.Task.Factory.StartNew(() => Do(index));
            });

            Console.ReadLine();
        }

        static private async System.Threading.Tasks.Task Do(int index)
        {
            for (int i = 0; i < 1000000; i++)
            {
                var key = Guid.NewGuid().ToString();
                var value = Guid.NewGuid().ToString();

                try
                {
                    await redis.Strings.Set(key, value);
                    Console.WriteLine(index.ToString() + ":" + i.ToString() + " - " + (Encoding.UTF8.GetString(await redis.Strings.Get(key)) == value).ToString());
                    await redis.Keys.Del(key);
                    await redis.Publish("channel1", "from " + index.ToString() + " " + value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
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
            var client = Redis.Driver.RedisClientFactory.Get("test1");

            //set key1 value1
            client.Strings.Set("key1", "value1").ContinueWith(c => Console.WriteLine(c.Exception.ToString()),
                System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

            //get key1 value
            client.Strings.Get("key1").ContinueWith(c =>
            {
                if (c.IsFaulted) Console.WriteLine(c.Exception.ToString());
                else Console.WriteLine(Encoding.UTF8.GetString(c.Result));
            });

            //remove key1
            Sodao.FastSocket.SocketBase.Utils.TaskEx.Delay(3000, () =>
            {
                client.Keys.Del("key1").ContinueWith(c =>
                {
                    if (c.IsFaulted) Console.WriteLine(c.Exception.ToString());
                    else Console.WriteLine("key1 was deleted.");
                });
            });

            Console.ReadLine();
        }
    }
}
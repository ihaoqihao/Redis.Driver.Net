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
            var client = Redis.Driver.RedisClientFactory.Get("test1");

            for (int i = 0; i < 10; i++)
            {
                new System.Threading.Thread(state =>
                {
                    int j = 0;
                    while (j++ < 1000)
                    {
                        client.Strings.Get("a").ContinueWith(c =>
                        {
                            if (c.IsFaulted)
                                Console.WriteLine(c.Exception.ToString());
                            else
                                Console.WriteLine("get key:a " + Encoding.UTF8.GetString(c.Result));
                        });

                        client.Strings.Append("a", "!").ContinueWith(c =>
                        {
                            if (c.IsFaulted)
                                Console.WriteLine(c.Exception.ToString());
                            else
                                Console.WriteLine("append key:a " + c.Result.ToString());
                        });
                    }
                })
                {
                    IsBackground = true
                }.Start();
            }
            Console.ReadLine();
        }
    }
}
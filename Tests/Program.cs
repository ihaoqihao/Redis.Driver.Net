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
                        client.Strings.Set(j.ToString(), Guid.NewGuid().ToString()).ContinueWith(c =>
                        {
                            Console.WriteLine((c as System.Threading.Tasks.Task<string>).Result);
                        });
                        client.Strings.Get(j.ToString()).ContinueWith(c =>
                        {
                            if (c.IsFaulted)
                                Console.WriteLine(c.Exception.ToString());
                            else
                                Console.WriteLine(Encoding.UTF8.GetString(c.Result));
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
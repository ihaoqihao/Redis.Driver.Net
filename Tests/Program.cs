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

            for (int i = 0; i < 20; i++)
            {
                int i2 = i;
                new System.Threading.Thread(state =>
                {
                    int j = 0;
                    while (j++ < 2000)
                    {
                        string id = Guid.NewGuid().ToString();
                        client.Strings.Set(i2.ToString() + j.ToString(), id).ContinueWith(c =>
                        {
                            //Console.WriteLine((c as System.Threading.Tasks.Task<string>).Result);
                        });
                        client.Strings.Get(i2.ToString() + j.ToString()).ContinueWith(c =>
                        {
                            if (c.IsFaulted)
                                Console.WriteLine(c.Exception.ToString());
                            else
                            {
                                if (Encoding.UTF8.GetString(c.Result) != id)
                                    Console.WriteLine("error");
                              //  Console.WriteLine(Encoding.UTF8.GetString(c.Result) == id);
                            }
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
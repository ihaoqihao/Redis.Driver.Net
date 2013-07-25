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
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            var client = Redis.Driver.RedisClientFactory.Get("test1");
            int i = 0;
            while (i++ < 5)
            {
                new System.Threading.Thread(_ =>
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    int j = 0;
                    while (j++ < 100000)
                    {
                        client.Strings.Set(j.ToString(), i.ToString() + j.ToString());
                    }
                    Console.WriteLine(sw.ElapsedMilliseconds);
                })
                {
                    IsBackground = true
                }.Start();
            }

            Console.ReadLine();
        }

        static void TaskScheduler_UnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Console.WriteLine(e.Exception.ToString());
        }
    }
}
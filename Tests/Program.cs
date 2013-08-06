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
            Console.ReadLine();
        }
    }
}
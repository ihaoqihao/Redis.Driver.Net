using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ReplyList
    {
        [Test]
        public void Test()
        {
            var list = new Redis.Driver.DefaultRedisReplyList();
            list.Enqueue(1);
            list.Enqueue(2);
            list.Enqueue(3);
            list.Enqueue(4);
            list.Enqueue(5);

            Assert.True(list.Dequeue() == 1);
            Assert.True(list.Dequeue() == 2);
            Assert.True(list.Pull() == 5);
        }
    }
}
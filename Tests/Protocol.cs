using System;
using System.Text;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Protocol
    {
        [Test]
        public void StatusReply()
        {
            string reply = "???+OK\r\n---";
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(reply), 3, 5);
            int readed;
            var objResponse = new Redis.Driver.RedisProtocol().FindResponse(null, buffer, out readed);
            Assert.AreEqual(readed, 5);
            Assert.IsNotNull(objResponse);
            Assert.IsTrue(objResponse.Reply is Redis.Driver.StatusReply);
            Assert.AreEqual("OK", (objResponse.Reply as Redis.Driver.StatusReply).Status);
        }

        [Test]
        public void ErrorReply()
        {
            string reply = "????-(error) ERR unknown command 'INC'\r\n----------";
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(reply), 4, 36);
            int readed;
            var objResponse = new Redis.Driver.RedisProtocol().FindResponse(null, buffer, out readed);
            Assert.AreEqual(readed, 36);
            Assert.IsNotNull(objResponse);
            Assert.IsTrue(objResponse.Reply is Redis.Driver.ErrorReply);
            Assert.AreEqual("(error) ERR unknown command 'INC'", (objResponse.Reply as Redis.Driver.ErrorReply).ErrorMessage);
        }

        [Test]
        public void IntegerReply()
        {
            string reply = "0000000:1000\r\n$$$$$";
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(reply), 7, 7);
            int readed;
            var objResponse = new Redis.Driver.RedisProtocol().FindResponse(null, buffer, out readed);
            Assert.AreEqual(readed, 7);
            Assert.IsNotNull(objResponse);
            Assert.IsTrue(objResponse.Reply is Redis.Driver.IntegerReply);
            Assert.AreEqual(1000, (objResponse.Reply as Redis.Driver.IntegerReply).Value);

            reply = "~~~~~:-1\r\n";
            buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(reply), 5, 5);
            objResponse = new Redis.Driver.RedisProtocol().FindResponse(null, buffer, out readed);
            Assert.AreEqual(readed, 5);
            Assert.IsNotNull(objResponse);
            Assert.IsTrue(objResponse.Reply is Redis.Driver.IntegerReply);
            Assert.AreEqual(-1, (objResponse.Reply as Redis.Driver.IntegerReply).Value);
        }

        [Test]
        public void BulkReply()
        {
            string reply = "#############$7\r\nmyvalue\r\n~~~~~~~~~~~";
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(reply), 13, 13);
            int readed;
            var objResponse = new Redis.Driver.RedisProtocol().FindResponse(null, buffer, out readed);
            Assert.IsNotNull(objResponse);
            Assert.AreEqual(readed, 13);
            Assert.IsTrue(objResponse.Reply is Redis.Driver.BulkReplies);
            Assert.AreEqual("myvalue", Encoding.UTF8.GetString((objResponse.Reply as Redis.Driver.BulkReplies).Payload));

            reply = "!!!$-1\r\n^^^^^^^^^^^^^^^";
            buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(reply), 3, 5);
            objResponse = new Redis.Driver.RedisProtocol().FindResponse(null, buffer, out readed);
            Assert.IsNotNull(objResponse);
            Assert.AreEqual(readed, 5);
            Assert.IsTrue(objResponse.Reply is Redis.Driver.BulkReplies);
            Assert.IsNull((objResponse.Reply as Redis.Driver.BulkReplies).Payload);
        }

        [Test]
        public void MultiBulkReply()
        {
            string reply = "@@@@@@@@@@*4\r\n$3\r\nfoo\r\n$-1\r\n$3\r\nbar\r\n:154\r\n";
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(reply), 10, 33);
            int readed;
            var objResponse = new Redis.Driver.RedisProtocol().FindResponse(null, buffer, out readed);
            Assert.IsNotNull(objResponse);
            Assert.AreEqual(readed, 33);
            Assert.IsTrue(objResponse.Reply is Redis.Driver.MultiBulkReplies);
            Assert.AreEqual(4, (objResponse.Reply as Redis.Driver.MultiBulkReplies).Replies.Length);

            //foreach (var child in (objResponse.Reply as Redis.Driver.MultiBulkReplies).Payloads)
            //{
            //    if (child == null)
            //        Console.WriteLine("nil");
            //    else
            //        Console.WriteLine(Encoding.UTF8.GetString(child));
            //}

            reply = "***********-1\r\n";
            buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(reply), 10, 5);
            objResponse = new Redis.Driver.RedisProtocol().FindResponse(null, buffer, out readed);
            Assert.IsNotNull(objResponse);
            Assert.AreEqual(readed, 5);
            Assert.IsTrue(objResponse.Reply is Redis.Driver.MultiBulkReplies);
            //Assert.IsTrue((objResponse.Reply as Redis.Driver.MultiBulkReplies).Payloads == null);
        }
    }
}
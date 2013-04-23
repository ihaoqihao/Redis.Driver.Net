using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Sodao.FastSocket.Client;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Utils;

namespace Redis.Driver
{
    /// <summary>
    /// redis subscriber
    /// </summary>
    public sealed class RedisSubscriber : BaseHost
    {
        #region Private Members
        private readonly RedisProtocol _protocol = new RedisProtocol();
        private IConnection _currentConnection = null;
        private readonly EndPoint _endPoint = null;

        private readonly HashSet<string> _setChannels = new HashSet<string>();
        private readonly HashSet<string> _setPatterns = new HashSet<string>();
        #endregion

        #region Events
        /// <summary>
        /// Listener event
        /// </summary>
        public event Action<string, byte[]> Listener;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="connectionString">连接字符串,如 redis://127.0.0.1:6379</param>
        /// <exception cref="ArgumentNullException">connectionString is null or empty.</exception>
        public RedisSubscriber(string connectionString)
            : base(1024, 1024)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            var arr = connectionString.Split(':');
            this._endPoint = new IPEndPoint(IPAddress.Parse(arr[1].Substring(2)), int.Parse(arr[2]));
            this.BeginConnect();
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public RedisSubscriber(string host, int port)
            : base(1024, 1024)
        {
            this._endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            this.BeginConnect();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// begin connect
        /// </summary>
        private void BeginConnect()
        {
            SocketConnector.BeginConnect(this._endPoint, this, connection =>
            {
                if (connection == null)
                {
                    //延时重连
                    TaskEx.Delay(3000, this.BeginConnect);
                    return;
                }

                base.RegisterConnection(connection);
            });
        }
        /// <summary>
        /// fire Listener
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="payload"></param>
        private void OnListener(string channel, byte[] payload)
        {
            if (this.Listener != null)
                this.Listener(channel, payload);
        }
        /// <summary>
        /// subscribe channel
        /// </summary>
        /// <param name="channels"></param>
        private void SubscribeInternal(string[] channels)
        {
            if (channels == null || channels.Length == 0)
                return;

            var connection = this._currentConnection;
            if (connection == null)
                return;

            var r = new RedisRequest(channels.Length + 1).AddArgument("SUBSCRIBE");
            foreach (var channel in channels)
                r.AddArgument(channel);

            connection.BeginSend(new Packet(r.ToPayload()));
        }
        /// <summary>
        /// unsubscribe channel
        /// </summary>
        /// <param name="channels"></param>
        private void UnSubscribeInternal(string[] channels)
        {
            if (channels == null || channels.Length == 0)
                return;

            var connection = this._currentConnection;
            if (connection == null)
                return;

            var r = new RedisRequest(channels.Length + 1).AddArgument("UNSUBSCRIBE");
            foreach (var channel in channels)
                r.AddArgument(channel);

            connection.BeginSend(new Packet(r.ToPayload()));
        }
        /// <summary>
        /// pattern subscribe
        /// </summary>
        /// <param name="patterns"></param>
        private void PatternSubscribeInternal(string[] patterns)
        {
            if (patterns == null || patterns.Length == 0)
                return;

            var connection = this._currentConnection;
            if (connection == null)
                return;

            var r = new RedisRequest(patterns.Length + 1).AddArgument("PSUBSCRIBE");
            foreach (var pattern in patterns)
                r.AddArgument(pattern);

            connection.BeginSend(new Packet(r.ToPayload()));
        }
        /// <summary>
        /// unsubscribe pattern
        /// </summary>
        /// <param name="patterns"></param>
        private void UnPatternSubscribeInternal(string[] patterns)
        {
            if (patterns == null || patterns.Length == 0)
                return;

            var connection = this._currentConnection;
            if (connection == null)
                return;

            var r = new RedisRequest(patterns.Length + 1).AddArgument("PUNSUBSCRIBE");
            foreach (var pattern in patterns)
                r.AddArgument(pattern);

            connection.BeginSend(new Packet(r.ToPayload()));
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// OnMessageReceived
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="e"></param>
        protected override void OnMessageReceived(IConnection connection, MessageReceivedEventArgs e)
        {
            base.OnMessageReceived(connection, e);

            int readLength;
            RedisResponse response = null;
            try
            {
                response = this._protocol.FindResponse(connection, e.Buffer, out readLength);
            }
            catch (Exception ex)
            {
                connection.BeginDisconnect(ex);
                e.SetReadlength(e.Buffer.Count);
                return;
            }

            if (response != null)
                ThreadPool.QueueUserWorkItem(c =>
                {
                    var objMulti = response.Reply as MultiBulkReplies;
                    if (objMulti == null || objMulti.Replies == null || objMulti.Replies.Length != 3)
                        return;

                    var objFlagBulk = objMulti.Replies[0] as BulkReplies;
                    if (objFlagBulk == null || objFlagBulk.Payload == null)
                        return;

                    try
                    {
                        if (Encoding.UTF8.GetString(objFlagBulk.Payload) != "message")
                            return;

                        var objChannelNameBulk = objMulti.Replies[1] as BulkReplies;
                        if (objChannelNameBulk == null || objChannelNameBulk.Payload == null)
                            return;

                        var objMessageBulk = objMulti.Replies[2] as BulkReplies;
                        if (objMessageBulk == null || objMessageBulk.Payload == null)
                            return;

                        this.OnListener(Encoding.UTF8.GetString(objChannelNameBulk.Payload), objMessageBulk.Payload);
                    }
                    catch { }
                });

            e.SetReadlength(readLength);
        }
        /// <summary>
        /// OnSendCallback
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="e"></param>
        protected override void OnSendCallback(IConnection connection, SendCallbackEventArgs e)
        {
            base.OnSendCallback(connection, e);
            //重发
            if (e.Status != SendCallbackStatus.Success && connection.Active)
                connection.BeginSend(e.Packet);
        }
        /// <summary>
        /// OnConnected
        /// </summary>
        /// <param name="connection"></param>
        protected override void OnConnected(IConnection connection)
        {
            base.OnConnected(connection);

            this._currentConnection = connection;
            connection.BeginReceive();

            string[] channels = null;
            string[] patterns = null;
            lock (this)
            {
                if (this._setChannels.Count > 0)
                {
                    channels = new string[this._setChannels.Count];
                    this._setChannels.CopyTo(channels);
                }

                if (this._setPatterns.Count > 0)
                {
                    patterns = new string[this._setPatterns.Count];
                    this._setPatterns.CopyTo(patterns);
                }
            }
            this.SubscribeInternal(channels);
            this.PatternSubscribeInternal(patterns);
        }
        /// <summary>
        /// OnDisconnected
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        protected override void OnDisconnected(IConnection connection, Exception ex)
        {
            base.OnDisconnected(connection, ex);

            this._currentConnection = null;
            this.BeginConnect();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// subscribe channel
        /// </summary>
        /// <param name="channels"></param>
        public void Subscribe(params string[] channels)
        {
            if (channels == null || channels.Length == 0)
                return;

            lock (this)
            {
                foreach (var c in channels)
                    this._setChannels.Add(c);
            }
            this.SubscribeInternal(channels);
        }
        /// <summary>
        /// unsubscribe channel
        /// </summary>
        /// <param name="channels"></param>
        public void UnSubscribe(params string[] channels)
        {
            if (channels == null || channels.Length == 0)
                return;

            lock (this)
            {
                foreach (var c in channels)
                    this._setChannels.Remove(c);
            }
            this.UnSubscribeInternal(channels);
        }
        /// <summary>
        /// pattern subscribe
        /// </summary>
        /// <param name="patterns"></param>
        public void PatternSubscribe(params string[] patterns)
        {
            if (patterns == null || patterns.Length == 0)
                return;

            lock (this)
            {
                foreach (var p in patterns)
                    this._setPatterns.Add(p);
            }
            this.PatternSubscribeInternal(patterns);
        }
        /// <summary>
        /// unsubscribe pattern
        /// </summary>
        /// <param name="patterns"></param>
        public void UnPatternSubscribe(params string[] patterns)
        {
            if (patterns == null || patterns.Length == 0)
                return;

            lock (this)
            {
                foreach (var p in patterns)
                    this._setPatterns.Remove(p);
            }
            this.UnPatternSubscribeInternal(patterns);
        }
        #endregion
    }
}
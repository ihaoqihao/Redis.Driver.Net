﻿using Sodao.FastSocket.Client;
using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

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

        private readonly Timer _timer = null;
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
            : base(8192, 8192)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");

            var arr = connectionString.Split(':');
            this._endPoint = new IPEndPoint(IPAddress.Parse(arr[1].Substring(2)), int.Parse(arr[2]));
            this.BeginConnect();
            this._timer = new Timer(_ => this.Ping(), null, 0, 1000 * 10);
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public RedisSubscriber(string host, int port)
            : base(8192, 8192)
        {
            this._endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            this.BeginConnect();
            this._timer = new Timer(_ => this.Ping(), null, 0, 1000 * 10);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// begin connect
        /// </summary>
        private void BeginConnect()
        {
            SocketConnector.Connect(this._endPoint).ContinueWith(c =>
            {
                if (c.IsFaulted)
                {
                    TaskEx.Delay(new Random().Next(1000, 3000)).ContinueWith(_ => this.BeginConnect());
                    return;
                }
                base.RegisterConnection(base.NewConnection(c.Result));
            });
        }
        /// <summary>
        /// on reponse
        /// </summary>
        /// <param name="message"></param>
        private void OnResponse(RedisMessage message)
        {
            if (message == null) return;

            var objMulti = message.Reply as MultiBulkReplies;
            if (objMulti == null || objMulti.Replies == null || objMulti.Replies.Length == 0) return;

            var objFlag = objMulti.Replies[0] as BulkReplies;
            if (objFlag == null || objFlag.Payload == null || objFlag.Payload.Length == 0) return;

            switch (Encoding.UTF8.GetString(objFlag.Payload))
            {
                case "message":
                    {
                        if (objMulti.Replies.Length != 3) return;

                        var objChannel = objMulti.Replies[1] as BulkReplies;
                        if (objChannel == null || objChannel.Payload == null || objChannel.Payload.Length == 0) return;

                        var objMsg = objMulti.Replies[2] as BulkReplies;
                        if (objMsg == null || objMsg.Payload == null) return;

                        this.OnListener(Encoding.UTF8.GetString(objChannel.Payload), objMsg.Payload);
                        break;
                    }
                case "pmessage":
                    {
                        if (objMulti.Replies.Length != 4) return;

                        var objChannel = objMulti.Replies[2] as BulkReplies;
                        if (objChannel == null || objChannel.Payload == null || objChannel.Payload.Length == 0) return;

                        var objMsg = objMulti.Replies[3] as BulkReplies;
                        if (objMsg == null || objMsg.Payload == null) return;

                        this.OnListener(Encoding.UTF8.GetString(objChannel.Payload), objMsg.Payload);
                        break;
                    }
                default: break;
            }
        }
        /// <summary>
        /// fire Listener
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="payload"></param>
        private void OnListener(string channel, byte[] payload)
        {
            if (this.Listener == null) return;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try { this.Listener(channel, payload); }
                catch (Exception ex) { Sodao.FastSocket.SocketBase.Log.Trace.Error(ex.Message, ex); }
            });
        }
        /// <summary>
        /// ping
        /// </summary>
        private void Ping()
        {
            var connection = this._currentConnection;
            if (connection == null) return;
            connection.BeginSend(new Packet(new RedisRequest(1).AddArgument("PING").ToPayload()));
        }
        /// <summary>
        /// subscribe channel
        /// </summary>
        /// <param name="channels"></param>
        private void SubscribeInternal(string[] channels)
        {
            if (channels == null || channels.Length == 0) return;

            var connection = this._currentConnection;
            if (connection == null) return;

            var r = new RedisRequest(channels.Length + 1).AddArgument("SUBSCRIBE");
            foreach (var channel in channels) r.AddArgument(channel);

            connection.BeginSend(new Packet(r.ToPayload()));
        }
        /// <summary>
        /// unsubscribe channel
        /// </summary>
        /// <param name="channels"></param>
        private void UnSubscribeInternal(string[] channels)
        {
            if (channels == null || channels.Length == 0) return;

            var connection = this._currentConnection;
            if (connection == null) return;

            var r = new RedisRequest(channels.Length + 1).AddArgument("UNSUBSCRIBE");
            foreach (var channel in channels) r.AddArgument(channel);

            connection.BeginSend(new Packet(r.ToPayload()));
        }
        /// <summary>
        /// pattern subscribe
        /// </summary>
        /// <param name="patterns"></param>
        private void PatternSubscribeInternal(string[] patterns)
        {
            if (patterns == null || patterns.Length == 0) return;

            var connection = this._currentConnection;
            if (connection == null) return;

            var r = new RedisRequest(patterns.Length + 1).AddArgument("PSUBSCRIBE");
            foreach (var pattern in patterns) r.AddArgument(pattern);

            connection.BeginSend(new Packet(r.ToPayload()));
        }
        /// <summary>
        /// unsubscribe pattern
        /// </summary>
        /// <param name="patterns"></param>
        private void UnPatternSubscribeInternal(string[] patterns)
        {
            if (patterns == null || patterns.Length == 0) return;

            var connection = this._currentConnection;
            if (connection == null) return;

            var r = new RedisRequest(patterns.Length + 1).AddArgument("PUNSUBSCRIBE");
            foreach (var pattern in patterns) r.AddArgument(pattern);

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
            RedisMessage message = null;
            try { message = this._protocol.Parse(connection, e.Buffer, out readLength); }
            catch (Exception ex)
            {
                base.OnConnectionError(connection, ex);
                connection.BeginDisconnect(ex);
                e.SetReadlength(e.Buffer.Count);
                return;
            }

            this.OnResponse(message);
            e.SetReadlength(readLength);
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

            TaskEx.Delay(new Random().Next(10, 100)).ContinueWith(_ => this.BeginConnect());
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// subscribe channel
        /// </summary>
        /// <param name="channels"></param>
        public void Subscribe(params string[] channels)
        {
            if (channels == null || channels.Length == 0) return;
            lock (this) { foreach (var c in channels) this._setChannels.Add(c); }
            this.SubscribeInternal(channels);
        }
        /// <summary>
        /// unsubscribe channel
        /// </summary>
        /// <param name="channels"></param>
        public void UnSubscribe(params string[] channels)
        {
            if (channels == null || channels.Length == 0) return;
            lock (this) { foreach (var c in channels) this._setChannels.Remove(c); }
            this.UnSubscribeInternal(channels);
        }
        /// <summary>
        /// pattern subscribe
        /// </summary>
        /// <param name="patterns"></param>
        public void PatternSubscribe(params string[] patterns)
        {
            if (patterns == null || patterns.Length == 0) return;
            lock (this) { foreach (var p in patterns) this._setPatterns.Add(p); }
            this.PatternSubscribeInternal(patterns);
        }
        /// <summary>
        /// unsubscribe pattern
        /// </summary>
        /// <param name="patterns"></param>
        public void UnPatternSubscribe(params string[] patterns)
        {
            if (patterns == null || patterns.Length == 0) return;
            lock (this) { foreach (var p in patterns) this._setPatterns.Remove(p); }
            this.UnPatternSubscribeInternal(patterns);
        }
        #endregion
    }
}
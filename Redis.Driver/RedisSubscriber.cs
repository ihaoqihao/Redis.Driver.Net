using System;
using System.Collections.Generic;
using System.Net;
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
        private readonly ISocketConnector _connector = null;
        private IConnection _currentConnection = null;

        private readonly HashSet<string> _channels = new HashSet<string>();
        private readonly HashSet<string> _patterns = new HashSet<string>();
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
        /// <param name="host"></param>
        /// <param name="port"></param>
        public RedisSubscriber(string host, int port)
            : base(1024, 1024)
        {
            this._connector = new DefaultSocketConnector(string.Concat(host, port.ToString()),
                new IPEndPoint(IPAddress.Parse(host), port), this);
            this._connector.Connected += new Action<ISocketConnector, IConnection>(this.Connector_Connected);
            this._connector.ConnectFailed += new Action<ISocketConnector>(this.Connector_ConnectFailed);
            this._connector.BeginConnect();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// connected
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="connection"></param>
        private void Connector_Connected(ISocketConnector connector, IConnection connection)
        {
            this._currentConnection = connection;
            base.RegisterConnection(connection);
        }
        /// <summary>
        /// connect failed
        /// </summary>
        /// <param name="connector"></param>
        private void Connector_ConnectFailed(ISocketConnector connector)
        {
            //延时重连
            TaskEx.Delay(3000, this._connector.BeginConnect);
        }
        #endregion

        #region Private Methods
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

            foreach (var c in channels)
                this._channels.Add(c);
        }
        /// <summary>
        /// pattern subscribe
        /// </summary>
        /// <param name="patterns"></param>
        public void PatternSubscribe(params string[] patterns)
        {
            if (patterns == null || patterns.Length == 0)
                return;

            foreach (var p in patterns)
                this._patterns.Add(p);
        }
        #endregion
    }
}
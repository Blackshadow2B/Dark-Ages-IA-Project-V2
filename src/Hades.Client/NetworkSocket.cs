﻿using System;
using System.Net.Sockets;
using Darkages.Network;

namespace DAClient
{
    public class NetworkSocket
    {
        public bool IsProxySocket { get; set; }

        internal Socket Socket;
        private const int HeaderLength = 3;

        private readonly byte[] _header = new byte[HeaderLength];
        private readonly byte[] _packet = new byte[0xFFFF];

        private int _headerOffset;
        private int _packetLength;
        private int _packetOffset;

        public NetworkSocket(Socket socket)
        {
            ConfigureTcpSocket(socket);
            Socket = socket;
        }

        public bool HeaderComplete => _headerOffset == HeaderLength;

        public bool PacketComplete => _packetOffset == _packetLength;

        public virtual IAsyncResult BeginReceiveHeader(AsyncCallback callback, out SocketError error, object state)
        {
            return Socket.BeginReceive(
                _header,
                _headerOffset,
                HeaderLength - _headerOffset,
                SocketFlags.None,
                out error,
                callback,
                state);
        }

        public virtual IAsyncResult BeginReceivePacket(AsyncCallback callback, out SocketError error, object state)
        {
            return Socket.BeginReceive(
                _packet,
                _packetOffset,
                _packetLength - _packetOffset,
                SocketFlags.None,
                out error,
                callback,
                state);
        }

        public virtual int EndReceiveHeader(IAsyncResult result, out SocketError error)
        {
            var bytes = Socket.EndReceive(result, out error);

            if (bytes == 0)
                return 0;

            _headerOffset += bytes;

            if (!HeaderComplete)
                return bytes;

            _packetLength = (_header[1] << 8) | _header[2];
            _packetOffset = 0;

            return bytes;
        }

        public virtual int EndReceivePacket(IAsyncResult result, out SocketError error)
        {
            var bytes = Socket.EndReceive(result, out error);

            if (bytes == 0)
                return 0;

            _packetOffset += bytes;

            if (PacketComplete) _headerOffset = 0;

            return bytes;
        }

        public NetworkPacket ToPacket()
        {
            return PacketComplete ? new NetworkPacket(_packet, _packetLength) : null;
        }

        private static void ConfigureTcpSocket(Socket tcpSocket)
        {
            tcpSocket.NoDelay = true;
            tcpSocket.ReceiveBufferSize = 0xFFFF;
            tcpSocket.SendBufferSize = 0xFFFF;
        }
    }
}
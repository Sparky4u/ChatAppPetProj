using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatClient.MVVM.Model;
using Shared.Net.IO;

namespace ChatClient.Net
{
    internal class Server : IDisposable
    {
        private readonly TcpClient _client;
        private PacketReader _packetReader;

        public Server()
        {
            _client = new TcpClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _packetReader.Dispose();
        }

        public event Action<UserModel> ConnectedEvent;
        public event Action<string> MsgReceivedEvent;
        public event Action<string> UserDisconnectEvent;

        public void ConnectToServer(string username)
        {
            if (_client.Connected)
                return;

            _client.Connect("127.0.0.1", 7891);
            _packetReader = new PacketReader(_client.GetStream());
            if (!string.IsNullOrEmpty(username))
            {
                var connectPacket = new PacketBuilder();
                connectPacket.WriteOpCode(0);
                connectPacket.WriteMessage(username);
                _client.Client.Send(connectPacket.GetPacketBytes());
            }

            ReadPackets();
        }

        public void SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(message);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var operationCode = _packetReader.ReadByte();
                    switch (operationCode)
                    {
                        case 1:
                            InvokeUserConnected();
                            break;
                        case 5:
                            InvokeMessageReceived();
                            break;
                        case 10:
                            InvokeUserDisconnected();
                            break;
                        default:
                            throw new NotSupportedException($"Unknown operation code {operationCode}");
                    }
                }
            });
        }

        private void InvokeMessageReceived()
        {
            var msg = _packetReader.ReadMessage();
            MsgReceivedEvent?.Invoke(msg);
        }

        private void InvokeUserDisconnected()
        {
            var uid = _packetReader.ReadMessage();
            UserDisconnectEvent?.Invoke(uid);
        }

        private void InvokeUserConnected()
        {
            var user = new UserModel
            {
                Username = _packetReader.ReadMessage(),
                UID = _packetReader.ReadMessage(),
            };
            ConnectedEvent?.Invoke(user);
        }
    }
}
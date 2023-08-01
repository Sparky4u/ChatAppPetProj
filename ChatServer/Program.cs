using System.Net;
using System.Net.Sockets;
using Shared.Net.IO;

namespace ChatServer
{
    public class Program
    {
        static List<Client> _users;

        public static void Main(string[] args)
        {
            _users = new List<Client>();
            var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            listener.Start();

            while (true)
            {
                var client = new Client(listener.AcceptTcpClient());
                _users.Add(client);

                /* Broadcast connection to everyone on the server */
                BroadcastConnection(client);
            }
        }

        static void BroadcastConnection(Client connectedClient)
        {
            foreach (var a in _users) //todo: remove second foreach loop
            {
                foreach (var usr in _users)
                {
                    var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(1);
                    broadcastPacket.WriteMessage(usr.Username);
                    broadcastPacket.WriteMessage(usr.Uid.ToString());
                    a.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                }
            }

            BroadcastMessage($"[{connectedClient.Username}: Connected]");
        }

        public static void BroadcastMessage(string message)
        {
            foreach (var user in _users)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(5);
                msgPacket.WriteMessage(message);
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }
        }

        public static void BroadcastDisconnect(string uid)
        {
            var disconnectedUser = _users.FirstOrDefault(x => x.Uid.ToString() == uid);

            if (disconnectedUser == null)
                return;

            _users.Remove(disconnectedUser);
            foreach (var user in _users)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(10);
                broadcastPacket.WriteMessage(uid);
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
            }

            BroadcastMessage($"[{disconnectedUser.Username}: Disconnected]");
        }
    }
}
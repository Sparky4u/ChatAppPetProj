using System.Net.Sockets;
using Shared.Net.IO;

namespace ChatServer
{
    public class Client
    {
        private readonly PacketReader _packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            Uid = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());
            Username = _packetReader.ReadMessage();

            Console.WriteLine($"[{DateTime.Now}]: Client has connected with the username: {Username}");

            Task.Run(Process);
        }

        public string Username { get; set; }
        public Guid Uid { get; set; }
        public TcpClient ClientSocket { get; set; }

        private void Process()
        {
            while (true)
                try
                {
                    var operationCode = _packetReader.ReadByte();
                    switch (operationCode)
                    {
                        case 5:
                            var msg = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}:Message received! {msg}]");
                            Program.BroadcastMessage($"[{DateTime.Now}] : [{Username}]: {msg} ");
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{Uid}: Disconnected!]");
                    Program.BroadcastDisconnect(Uid.ToString());
                    ClientSocket.Close();
                    break;
                }
        }
    }
}
using System.Net.Sockets;
using System.Text;

namespace Shared.Net.IO
{
    public class PacketReader : BinaryReader
    {
        private readonly NetworkStream _networkStream;

        public PacketReader(NetworkStream networkStream) : base(networkStream)
        {
            _networkStream = networkStream;
        }

        public string ReadMessage()
        {
            var length = ReadInt32();
            var msgBuffer = new byte[length];
            _networkStream.Read(msgBuffer, 0, length);

            var msg = Encoding.UTF8.GetString(msgBuffer);

            return msg;
        }
    }
}

using System.Text;

namespace Shared.Net.IO
{
    public class PacketBuilder
    {
        private readonly MemoryStream _memoryStream;

        public PacketBuilder()
        {
            _memoryStream = new MemoryStream();
        }

        public void WriteOpCode(byte operationCode)
        {
            _memoryStream.WriteByte(operationCode);
        }

        public void WriteMessage(string message)
        {
            var messageLength = message.Length;
            _memoryStream.Write(BitConverter.GetBytes(messageLength));
            _memoryStream.Write(Encoding.UTF8.GetBytes(message));
        }

        public byte[] GetPacketBytes()
        {
            return _memoryStream.ToArray();
        }
    }
}

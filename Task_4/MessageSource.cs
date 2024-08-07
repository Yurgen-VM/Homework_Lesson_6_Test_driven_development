using System.Net;
using System.Net.Sockets;
using System.Text;
using Task_4.Abstraction;

namespace Task_4
{
    internal class MessageSource : IMessageSource
    {
        private readonly UdpClient udpClient;
        public MessageSource(int port)
        {
            udpClient = new UdpClient(port);
        }

        public MessagePack ReceiveMessage(ref IPEndPoint endPoint)
        {
            byte[] data = udpClient.Receive(ref endPoint);
            string msgJson = Encoding.UTF8.GetString(data);
            return MessagePack.FromJson(msgJson);
        }


        public void SendMessage(MessagePack message, IPEndPoint endPoint)
        {
            string strJson = message.ToJson();
            byte[] data = Encoding.UTF8.GetBytes(strJson);
            udpClient.Send(data, data.Length, endPoint);
        }
    }
}

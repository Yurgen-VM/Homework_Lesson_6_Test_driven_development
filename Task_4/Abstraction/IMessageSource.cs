using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Task_4.Abstraction
{
    public interface IMessageSource
    {
        public void SendMessage(MessagePack message, IPEndPoint endPoint);
        public MessagePack ReceiveMessage(ref IPEndPoint endPoint);
    }
}

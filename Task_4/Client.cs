using System.Net;
using System.Net.Sockets;
using System.Text;
using Task_4.Abstraction;
using Task_4.Models;

namespace Task_4
{
    public class Client
    {
        private readonly string _name;
        private readonly IMessageSource _messageSource;
        private readonly IPEndPoint _endPoint;
        private readonly IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 54321);
        public Client(IMessageSource messageSource, IPEndPoint endPoint, string Name)
        {
            _messageSource = messageSource;
            _endPoint = endPoint;
            _name = Name;
        }

        public void Register()
        {
            var msgJson = new MessagePack()
            {
                Command = Command.Register,
                FromName = _name
            };
            _messageSource.SendMessage(msgJson, serverEP);
        }

        public void GetListNotConfirm()
        {
            var msgJson = new MessagePack()
            {
                Command = Command.ListNotConfirmation,
                FromName = _name
            };
            _messageSource.SendMessage(msgJson, serverEP);
        }


        public void ClientSender()
        {
            while (true)
            {
                Console.WriteLine("Введите сообщение");
                string? message = Console.ReadLine();
                Console.WriteLine("Укажите имя получателя");
                string? recipient = Console.ReadLine();
                if (string.IsNullOrEmpty(recipient))
                {
                    continue;
                }
                var msgJson = new MessagePack()
                {
                    Text = message,
                    FromName = _name,
                    ToName = recipient,
                    Command = Command.Message  

                };
                _messageSource.SendMessage(msgJson, serverEP);
            }
        }

        public void ClientListener()
        {            
            Register();
            GetListNotConfirm();
            IPEndPoint endPoint = new IPEndPoint(_endPoint.Address, _endPoint.Port);
            
            while (true)
            {
                MessagePack msg = _messageSource.ReceiveMessage(ref endPoint);
                var msgJson = new MessagePack()
                {                    
                    Id = msg.Id,
                    FromName = _name,                    
                    Command = Command.Confirmation
                };
                _messageSource.SendMessage(msgJson, endPoint);
                Console.WriteLine(msg.ToString());
            }            
        }

        public void Start()
        {
            //UdpClient udpClient = new UdpClient();
            new Thread(() => ClientListener()).Start();
            ClientSender();

        }
    }
}

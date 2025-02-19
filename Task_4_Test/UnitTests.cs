using Moq;
using System.Net;
using Task_4;
using Task_4.Abstraction;

namespace Task_4_Test
{
    public class Tests
    {
        private IMessageSource _source;
        private IPEndPoint _endPoint;        
        private Mock<IMessageSource> messageSourceMock;

        MessagePack testRegister = new MessagePack() // пакет сообщения для проверки метода TestClientMessageRegister
        {
            Command = Command.Register,
            FromName = "TestRegister", 
        };

        MessagePack testGetList = new MessagePack() // пакет сообщения для проверки метода TestClientMessageRegister
        {
            Command = Command.ListNotConfirmation,
            FromName = "TestGetList",
        };


        [SetUp]
        public void Setup()
        {
            _endPoint = new IPEndPoint(IPAddress.Any, 0);
            messageSourceMock = new Mock<IMessageSource>(); // создаем мок типа IMessageSource           
        }

        [Test]
        public void TestReceiveMessage()
        {
            _source = new MockMessageSource();
            var result = _source.ReceiveMessage(ref _endPoint);

            Assert.IsNotNull(result);
            Assert.IsNull(result.Text);
            Assert.IsNotNull(result.FromName);
            Assert.That("Вася", Is.EqualTo(result.FromName));
            Assert.That(Command.Register, Is.EqualTo(result.Command));
        }


        //Тест для проверки метoда Register. Тестируем верно ли Client формирует команду регистрации
        [Test]
        public void TestClientMessageRegister()
        {
                   
            Client client = new Client(messageSourceMock.Object, _endPoint, testRegister.FromName); // создаем тестового клиента, имя клиента получаем из предустановленного объекта testRegister 
            client.Register(); // Вызываем метод регистрации. 

            //Проверяем, что метод SendMessage имеет правильные параметры

            messageSourceMock.Verify(x => x.SendMessage(
                It.Is<MessagePack>
                (
                    msg => msg.Command == Command.Register && msg.FromName == "TestRegister"
                ),
                It.Is<IPEndPoint>
                (
                    ep => ep.Equals(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 54321))
                )
            ), Times.Once);
        }

        //Тест для проверки метoда GetListNotConfirm. Тестируем верно ли Client формирует команду запроса непрочитанных сообщений
        [Test]
        public void TestClientMessageGetListNotConfirm()
        {

            Client client = new Client(messageSourceMock.Object, _endPoint, testGetList.FromName); // создаем тестового клиента, имя клиента получаем из предустановленного объекта testGetList
            client.GetListNotConfirm(); // Вызываем метод запроса непрочитанных сообщений. 

            //Проверяем, что метод SendMessage имеет правильные параметры

            messageSourceMock.Verify(x => x.SendMessage(
                It.Is<MessagePack>
                (
                    msg => msg.Command == Command.ListNotConfirmation && msg.FromName == "TestGetList"
                ),
                It.Is<IPEndPoint>
                (
                    ep => ep.Equals(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 54321))
                )
            ), Times.Once);
        }

    }

    public class MockMessageSource : IMessageSource
    {
        public Queue<MessagePack> messages = new();

        public MockMessageSource()
        {
            messages.Enqueue(new MessagePack { Command = Command.Register, FromName = "Вася" });
            messages.Enqueue(new MessagePack { Command = Command.Register, FromName = "Юля" });
            messages.Enqueue(new MessagePack { Command = Command.Message, FromName = "Вася", ToName = "Юля", Text = "От Васи" });
            messages.Enqueue(new MessagePack { Command = Command.Message, FromName = "Юля", ToName = "Вася", Text = "От Юли" });
        }

        public MessagePack ReceiveMessage(ref IPEndPoint endPoint)
        {
            return messages.Peek();
        }

        public void SendMessage(MessagePack message, IPEndPoint endPoint)
        {
            messages.Enqueue(message);
        }
    }
}
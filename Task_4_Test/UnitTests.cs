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
        private IPEndPoint serverEP; // IPEndPoint сервера для тестирования клиента
        private Mock<IMessageSource> messageSourceMock;

        MessagePack testRegister = new MessagePack()
        {
            Command = Command.Register,
            FromName = "test",
        };


        [SetUp]
        public void Setup()
        {
            _endPoint = new IPEndPoint(IPAddress.Any, 0);
            messageSourceMock = new Mock<IMessageSource>(); // создаем мок типа IMessageSource   
            serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 54321); // инициализируем IPEndPoint сервера
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

            Client client = new Client(messageSourceMock.Object, serverEP, "Test"); // создаем тестового клиента
            client.Register(); // Вызываем метод регистрации. 

            //Проверяем, что метод SendMessage имеет правильные параметры

            messageSourceMock.Verify(x => x.SendMessage(
                It.Is<MessagePack>
                (
                    msg => msg.Command == Command.Register && msg.FromName == "Test"
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

            Client client = new Client(messageSourceMock.Object, serverEP, "Test"); // создаем тестового клиента
            client.GetListNotConfirm(); // Вызываем метод регистрации. 

            //Проверяем, что метод SendMessage имеет правильные параметры

            messageSourceMock.Verify(x => x.SendMessage(
                It.Is<MessagePack>
                (
                    msg => msg.Command == Command.ListNotConfirmation && msg.FromName == "Test"
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
using System.Net;
using Task_4.Abstraction;

namespace Task_4
{

    internal class Program
    {
        static async Task Main(string[] args)
        {
            
            if (args.Length < 2)
            {
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 54321);
                IMessageSource messageSource = new MessageSource(54321);
                Server server = new Server(messageSource,serverEP,"Server");
                server.Work();
            }
            else
            {
                int localPort = int.Parse(args[1]); // Водим с консоли номер порта для приема сообщений вторым аргументом 
                IPEndPoint clientEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), localPort);
                IMessageSource messageSource = new MessageSource(localPort); //Создаем объект реализующий интерфейс ImessageSource
                                                                             //В конструкторе содаем UdpClient с предустановленным портом для входящих   
                Client client = new Client(messageSource, clientEP, args[0]); // Создаем экземпляр клиента                    

                var ListenTask = Task.Run(() => client.ClientListener()); // В отдельном потоке запускаем метод для приема сообщений
                var SendTask = Task.Run(() => client.ClientSender()); // Запускаем метод для отправки сообщений         
                await Task.WhenAll(SendTask, ListenTask);
            }           

            await Console.Out.WriteLineAsync("Нажмите Enter для выхода");
            Console.ReadLine();

        }
    }
}

﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using Task_4.Abstraction;
using Task_4.Models;

namespace Task_4
{
    public class Server
    {      
        // Создадим словарь для хранения адресов клиентов по их именам

        public Dictionary<string, IPEndPoint> clients = new Dictionary<string, IPEndPoint>();       

        private readonly string _name;
        private readonly IMessageSource _messageSource;
        private readonly IPEndPoint _endPoint;  
        
        public Server(IMessageSource messageSource, IPEndPoint endPoint, string Name)
        {
            _messageSource = messageSource;
            _endPoint = endPoint;
            _name = Name;
        }
        

        // Метод для обработки регистрации нового клиента

        public void Register(MessagePack message, IPEndPoint senderEP)
        {
            Console.WriteLine("Зарегистрирован новый клиент: " + message.FromName);
            
            //Добавляем клиента в словарь
            clients.Add(message.FromName, senderEP);

            //Добавляем пользователя в базу данных (если его еще там нет)
            using (MesContext ctx = new MesContext())
            {
                if (ctx.Users.FirstOrDefault(x => x.Name == message.FromName) != null) return;
                ctx.Add(new User { Name = message.FromName });
                ctx.SaveChanges();
            }
        }

        // Метод для подтверждения получения сообщения
        public void ConfirmMessageReceived(int? id)
        {
            Console.WriteLine("Сообщение принято id = " + id);

            // Изменяем статус получения сообщения в базе данных
            using (MesContext ctx = new MesContext())
            {
                var msg = ctx.Messages.FirstOrDefault(x => x.Id == id);
                if (msg != null)
                {
                    msg.Received = true;
                    ctx.SaveChanges();
                }
            }
        }

        // Метод для пересылки сообщения
        public void RelyMessage(MessagePack message)
        {
            int? id = null;
            if (clients.TryGetValue(message.ToName, out IPEndPoint senderEP))
            {
                // Добавляем сообщение в базу данных
                using (MesContext ctx = new MesContext())
                {
                    var fromUser = ctx.Users.FirstOrDefault(x => x.Name == message.FromName);
                    var toUser = ctx.Users.FirstOrDefault(x => x.Name == message.ToName);
                    var msg = new Task_4.Models.Message
                    {
                        FromUser = fromUser,
                        ToUser = toUser,
                        Received = false,
                        Text = message.Text
                    };
                    ctx.Messages.Add(msg);
                    ctx.SaveChanges();
                    id = msg.Id;
                }

                // Подготавливаем сообщение для пересылки
                
                var forwardMessageJson = new MessagePack()
                {
                    Id = id,
                    Command = Command.Message,
                    ToName = message.ToName,
                    FromName = message.FromName,
                    Text = message.Text
                };

                // Отправляем сообщение клиенту

                _messageSource.SendMessage(forwardMessageJson, senderEP); // Используем экземпляр класса MessageSource реализующего методы интерфейса IMessageSource 

                Console.WriteLine($"Сообщение переслано от {message.FromName} для {message.ToName}");
            }
            else
            {
                Console.WriteLine("Пользователь не найден.");
            }
        }


        // Метод для отправки списка непрочитанных сообщений на запрос (команду) List

        public void SenderUnansweredMessages(MessagePack message)
        {
            if (clients.TryGetValue(message.FromName, out IPEndPoint senderEP)) // получаем EndPoint отправителя запроса
            {
                using (MesContext ctx = new MesContext()) // создаем экземпляр класса для инициализации подключения к БД
                {
                    var user = ctx.Users.FirstOrDefault(x => x.Name == message.FromName); // Из полученного сообщения берем имя отправителя, ищем его в БД и сохраняем его значение в переменную user

                    if (user != null)
                    {
                        List<Message> UnansweredMessages = new List<Message>(); // Создаем список непрочитанных сообщений адресованных отправителю запроса
                        UnansweredMessages = ctx.Messages.Where(m => m.ToUserId == user.Id && m.Received == false).ToList(); // добавляем в список сообщения адресованные отправителю запроса
                                                                                                                               // и имеющией статус неодоставленных (Received == false)

                        // Подготавливаем каждое сообщение из списка для отправки
                        foreach (var msg in UnansweredMessages)
                        {                          
                            var forwardMessageJson = new MessagePack()
                            {
                                Id = msg.Id,
                                Command = Command.Message,
                                ToName = user.Name,  // Имя получателя
                                FromName = msg.FromUser.Name, // Имя отправителя
                                Text = msg.Text
                            };
                            _messageSource.SendMessage(forwardMessageJson, senderEP); // Используем экземпляр класса MessageSource реализующего методы интерфейса IMessageSource 
                        }
                    }
                }
            }           
        }


        // Метод для обработки полученного сообщения
        void ProcessMessage(MessagePack message, IPEndPoint fromep)
        {
            Console.WriteLine($"Получено сообщение от {message.FromName} для {message.ToName} с командой {message.Command}:");
            Console.WriteLine(message.Text);

            // Обработка в зависимости от команды сообщения
            if (message.Command == Command.Register)
            {
                Register(message, new IPEndPoint(fromep.Address, fromep.Port));
            }
            if (message.Command == Command.Confirmation)
            {
                Console.WriteLine("Прием подтвержден");
                ConfirmMessageReceived(message.Id);
            }
            if (message.Command == Command.Message)
            {
                RelyMessage(message);
            }
            if (message.Command == Command.ListNotConfirmation)
            {               
                SenderUnansweredMessages(message); // Отправка списка непрочитанных сообщений клиенту
            }
        }

        // Метод для запуска работы сервера
        public void Work()
        {
            // Инициализация объекта для приема данных по UDP
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("UDP Сервер ожидает сообщений...");

            // Бесконечный цикл приема сообщений

            while (true)
            {          
                try
                {
                    MessagePack msg = _messageSource.ReceiveMessage(ref remoteEndPoint); // Используем экземпляр класса MessageSource реализующего методы интерфейса IMessageSource                                       
                    ProcessMessage(msg, remoteEndPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при обработке сообщения: " + ex.Message);
                }
            }
        }
    }
}

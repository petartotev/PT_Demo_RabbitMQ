﻿using RabbitMQ.Client;
using System.Reflection;
using System.Text;

Console.WriteLine($"{Assembly.GetExecutingAssembly().FullName?.Split(',').FirstOrDefault()} program started...");

var factory = new ConnectionFactory { HostName = "localhost" };

using (var connection = factory.CreateConnection())
{
    using (var channel = connection.CreateModel())
    {
        // ExchangeType.Fanout broadcasts messages to all Consumers. (4 types - Direct, Fanout, Headers, Topic)
        channel.ExchangeDeclare(exchange: "pubsub", type: ExchangeType.Fanout);

        while (true)
        {
            var message = Console.ReadLine();
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "pubsub", routingKey: "", basicProperties: null, body: body);

            Console.WriteLine("LogEmitter sent message '{0}' to LogReceiver.", message);
        }
    }
}

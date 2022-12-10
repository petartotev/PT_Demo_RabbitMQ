using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Reflection;

Console.WriteLine($"{Assembly.GetExecutingAssembly().FullName?.Split(',').First()} program started...");

var factory = new ConnectionFactory { HostName = "localhost" };

using (var connection = factory.CreateConnection())
{
    using (var channel = connection.CreateModel())
    {
        channel.QueueDeclare(queue: "helloworld", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(" Receiver received message '{0}' from Publisher.", message);
        };

        // Message Acknowledgement is sent by Consumer AUTOMATICALLY to inform RabbitMQ that the message has been received and processed.
        // When RabbitMQ receives such Message Acknowledgement, it can safely remove this message from queue.
        // autoAck: true => Consumer sends Message Acknowledgement to RabbitMQ AUTOMATICALLY.
        channel.BasicConsume(queue: "helloworld", autoAck: true, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}

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
        channel.QueueDeclare(queue: "workqueues", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var sleepMilliseconds = 1000 * message.Split('.').Length - 1;

            Console.WriteLine($"Worker will sleep for {sleepMilliseconds / 1000} seconds.");
            Thread.Sleep(sleepMilliseconds);

            Console.WriteLine(" Worker received message '{0}' from TaskScheduler.", message);

            // Message Acknowledgement is sent by Worker MANUALLY to inform RabbitMQ that the message has been received and processed.
            // When RabbitMQ receives such Message Acknowledgement, it can safely remove this message from queue.
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        // autoAck: false => Worker sends Message Acknowledgement to RabbitMQ MANUALLY.
        channel.BasicConsume(queue: "workqueues", autoAck: false, consumer: consumer);


        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}

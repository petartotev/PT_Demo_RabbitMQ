using RabbitMQ.Client;
using System.Reflection;
using System.Text;

Console.WriteLine($"{Assembly.GetExecutingAssembly().FullName?.Split(',').First()} program started...");

var factory = new ConnectionFactory { HostName = "localhost" };

using (var connection = factory.CreateConnection())
{
    using (var channel = connection.CreateModel())
    {
        channel.QueueDeclare(queue: "helloworld", durable: false, exclusive: false, autoDelete: false, arguments: null);

        while (true)
        {
            var message = Console.ReadLine();
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: "helloworld", basicProperties: null, body: body);

            Console.WriteLine(" Publisher sent message '{0}' to Receiver.", message);
        }
    }
}

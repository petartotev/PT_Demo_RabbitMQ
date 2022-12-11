using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Reflection;

var logLevels = new List<string> { "Error", "Warning", "Info", "Debug" };

Console.WriteLine($"{Assembly.GetExecutingAssembly().FullName?.Split(',').FirstOrDefault()} program started...");

var factory = new ConnectionFactory { HostName = "localhost" };

using (var connection = factory.CreateConnection())
{
    using (var channel = connection.CreateModel())
    {
        channel.ExchangeDeclare(exchange: "routing", type: ExchangeType.Direct);

        var queueName = channel.QueueDeclare().QueueName;

        // This Queue will receive ALL messages (with routingKey "Error", "Warning", "Info" or "Debug") from Direct Exchange "routing".
        foreach (var logLevel in logLevels)
        {
            channel.QueueBind(queue: queueName, exchange: "routing", routingKey: logLevel);
        }

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine("DirectLogReceiverAll received message '{0}' from DirectLogEmitter through Direct Exchange 'routing'.", message);
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        Console.WriteLine("Press [enter] to exit.");
        Console.ReadLine();
    }
}

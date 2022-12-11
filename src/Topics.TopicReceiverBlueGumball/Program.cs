using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Reflection;

Console.WriteLine($"{Assembly.GetExecutingAssembly().FullName?.Split(',').FirstOrDefault()} program started...");

var factory = new ConnectionFactory { HostName = "localhost" };

using (var connection = factory.CreateConnection())
{
    using (var channel = connection.CreateModel())
    {
        channel.ExchangeDeclare(exchange: "topics", type: ExchangeType.Topic);

        var queueName = channel.QueueDeclare().QueueName;

        // This Queue will receive only messages with routingKey "Blue.*.Gumball" from Direct Exchange "topics".
        channel.QueueBind(queue: queueName, exchange: "topics", routingKey: "Blue.*.Gumball");

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine("TopicReceiverBlueGumball received message '{0}' from TopicEmitter through Direct Exchange 'topics'.", message);
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        Console.WriteLine("Press [enter] to exit.");
        Console.ReadLine();
    }
}

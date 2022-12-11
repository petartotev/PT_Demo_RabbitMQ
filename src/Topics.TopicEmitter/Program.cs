using RabbitMQ.Client;
using System.Reflection;
using System.Text;

var colors = new List<string> { "Orange", "Blue", "Yellow" };
var animals = new List<string> { "Dog", "Cat", "Fish" };
var names = new List<string> { "Darwin", "Dexter", "Gumball" };
var random = new Random();

Console.WriteLine($"{Assembly.GetExecutingAssembly().FullName?.Split(',').FirstOrDefault()} program started...");

var factory = new ConnectionFactory { HostName = "localhost" };

using (var connection = factory.CreateConnection())
{
    using (var channel = connection.CreateModel())
    {
        // ExchangeType.Topic (4 types - Direct, Fanout, Headers, Topic)
        channel.ExchangeDeclare(exchange: "topics", type: ExchangeType.Topic);

        while (true)
        {
            var message = Console.ReadLine();
            var body = Encoding.UTF8.GetBytes(message);
            var randomType = string.Join('.', colors[random.Next(0, colors.Count)], animals[random.Next(0, animals.Count)], names[random.Next(0, names.Count)]);

            // Direct Exchange "topics" sends messages to Queues that have Binding Key which matches the Routing Key of the message.
            channel.BasicPublish(exchange: "topics", routingKey: randomType, basicProperties: null, body: body);

            Console.WriteLine("TopicLogEmitter sent message '{0}' of type '{1}' to Direct Exchange 'topics'.", message, randomType);
        }
    }
}

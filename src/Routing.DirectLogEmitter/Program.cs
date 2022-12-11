using RabbitMQ.Client;
using System.Reflection;
using System.Text;

var logLevels = new List<string> { "Error", "Warning", "Info", "Debug" };
var random = new Random();

Console.WriteLine($"{Assembly.GetExecutingAssembly().FullName?.Split(',').FirstOrDefault()} program started...");

var factory = new ConnectionFactory { HostName = "localhost" };

using (var connection = factory.CreateConnection())
{
    using (var channel = connection.CreateModel())
    {
        // ExchangeType.Direct (4 types - Direct, Fanout, Headers, Topic)
        channel.ExchangeDeclare(exchange: "routing", type: ExchangeType.Direct);

        while (true)
        {
            var message = Console.ReadLine();
            var body = Encoding.UTF8.GetBytes(message);
            var severity = logLevels[random.Next(0, logLevels.Count)];

            // Direct Exchange "routing" sends messages to Queues that have Binding Key which matches the Routing Key of the message.
            channel.BasicPublish(exchange: "routing", routingKey: severity, basicProperties: null, body: body);

            Console.WriteLine("DirectLogEmitter sent message '{0}' with severity '{1}' to Direct Exchange 'routing'.", message, severity);
        }
    }
}

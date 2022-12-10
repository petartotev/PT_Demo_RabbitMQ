using RabbitMQ.Client;
using System.Reflection;
using System.Text;

namespace WorkQueues.TaskScheduler;

public class Program
{
    private static readonly Random _random = new();

    public static void Main()
    {
        Console.WriteLine($"{Assembly.GetExecutingAssembly().FullName?.Split(',').First()} program started...");

        var factory = new ConnectionFactory { HostName = "localhost" };

        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "workqueues", durable: false, exclusive: false, autoDelete: false, arguments: null);

                while (true)
                {
                    var message = Console.ReadLine() + new string('.', _random.Next(30, 61));
                    var body = Encoding.UTF8.GetBytes(message);

                    // TaskScheduler uses 'Round-robin' distribution by default.
                    channel.BasicPublish(exchange: "", routingKey: "workqueues", basicProperties: null, body: body);

                    Console.WriteLine(" TaskScheduler sent message '{0}' to Worker.", message);
                }
            }
        }

    }
}
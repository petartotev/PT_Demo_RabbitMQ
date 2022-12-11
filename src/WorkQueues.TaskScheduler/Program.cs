using RabbitMQ.Client;
using System.Reflection;
using System.Text;

namespace WorkQueues.TaskScheduler;

public class Program
{
    private static readonly Random _random = new();

    public static void Main()
    {
        Console.WriteLine($"{Assembly.GetExecutingAssembly().FullName?.Split(',').FirstOrDefault()} program started...");

        var factory = new ConnectionFactory { HostName = "localhost" };

        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                // Durability | In order to make sure that queue survives a RabbitMQ node restart, we need to declare it as durable (durable: true).
                // Durability | RabbitMQ doesn't allow one to redefine an existing queue with different parameters and will return an error.
                // Durability | So, rename the queue (queue: "workqueues2") to declare new one.
                channel.QueueDeclare(queue: "workqueues", durable: false, exclusive: false, autoDelete: false, arguments: null);

                while (true)
                {
                    var message = Console.ReadLine() + new string('.', _random.Next(30, 61));
                    var body = Encoding.UTF8.GetBytes(message);

                    // Durability | We can mark our messages as persistent in order to guarantee that a message won't be lost.
                    // var properties = channel.CreateBasicProperties().Persistent = true;

                    // TaskScheduler uses 'Round-robin' distribution by default.
                    channel.BasicPublish(exchange: "", routingKey: "workqueues", basicProperties: null, body: body);

                    Console.WriteLine("TaskScheduler sent message '{0}' to Worker.", message);
                }
            }
        }

    }
}

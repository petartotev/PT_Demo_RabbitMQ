using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Reflection;

namespace RPC.RpcServer;

public class Program
{
    public static void Main()
    {
        Console.WriteLine($"{Assembly.GetExecutingAssembly().FullName?.Split(',').FirstOrDefault()} program started...");

        var factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };

        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "rpc_queue_new", durable: false, exclusive: false, autoDelete: false, arguments: null);

                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);

                channel.BasicConsume(queue: "rpc_queue_new", autoAck: false, consumer: consumer);

                Console.WriteLine("[x] Awaiting RPC requests");

                consumer.Received += (model, ea) =>
                {
                    string response = null;
                    var body = ea.Body.ToArray();
                    var props = ea.BasicProperties;

                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(body);

                        Console.WriteLine("[.] Fibonacci({0})", message);

                        int n = int.Parse(message);
                        response = Fibonacci(n).ToString();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[.] " + e.Message);
                        response = "";
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);

                        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                };

                Console.WriteLine("Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }

    private static int Fibonacci(int n)
    {
        if (n == 0 || n == 1)
        {
            return n;
        }

        return Fibonacci(n - 1) + Fibonacci(n - 2);
    }
}
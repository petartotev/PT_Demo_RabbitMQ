using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;

namespace RPC.RpcClient
{
    public class RpcClient
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;
        private readonly BlockingCollection<string> _responseQueue = new();
        private readonly IBasicProperties _props;

        public RpcClient()
        {
            var factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _replyQueueName = _channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(_channel);

            _props = _channel.CreateBasicProperties();

            var correlationId = Guid.NewGuid().ToString();

            _props.CorrelationId = correlationId;
            _props.ReplyTo = _replyQueueName;

            _consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);

                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    _responseQueue.Add(response);
                }
            };

            _channel.BasicConsume(consumer: _consumer, queue: _replyQueueName, autoAck: true);
        }

        public string Call(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "", routingKey: "rpc_queue_new", basicProperties: _props, body: messageBytes);

            return _responseQueue.Take();
        }

        public void Close()
        {
            _connection.Close();
        }
    }
}

using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMq.Trigger.Extension
{
    public class RabbitMqTriggerListener : IListener
    {
        private readonly ITriggeredFunctionExecutor _executor;
        private readonly RabbitMqTriggerAttribute _attribute;

        IModel _model { get; }
        IBasicPublishBatch _createBasicPublishBatch => CreateBasicPublishBatch();
        private string _connectionString;
        private string _hostName;
        private string _queueName;
        private string _userName;
        private string _password;
        private int _port;
        private string _deadLetterExchangeName;

        private string _consumerTag;
        private bool _disposed;
        private bool _started;
        private EventingBasicConsumer _consumer;

        private readonly ILogger _logger;

        public RabbitMqTriggerListener(ITriggeredFunctionExecutor executor, RabbitMqTriggerAttribute attribute, ILogger logger)
        {
            _executor = executor;
            _attribute = attribute;
            _logger = logger;
            _queueName = _attribute.QueueName;

            attribute.LoadRabbitSettings();

            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.Uri = GenerateRabbitHostUri();

            _model = connectionFactory.CreateConnection().CreateModel();

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (_started)
            {
                throw new InvalidOperationException("The listener has already been started.");
            }

            _model.QueueDeclare(_attribute.QueueName, false, false, false);

            _consumer = new EventingBasicConsumer(_model);

            _consumer.Received += async (model, ea) =>
            {
                FunctionResult result = await _executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = ea }, cancellationToken);

                if (result.Succeeded)
                {
                    _model.BasicAck(ea.DeliveryTag, false);
                }
                else
                {
                    if (ea.BasicProperties.Headers == null || !ea.BasicProperties.Headers.ContainsKey("requeueCount"))
                    {
                        CreateHeadersAndRepublish(ea);
                    }
                    else
                    {
                        RepublishMessages(ea);
                    }
                }
            };

            _consumerTag = _model.BasicConsume(queue: _queueName, autoAck: false, consumer: _consumer);

            _started = true;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (!_started)
            {
                throw new InvalidOperationException("The listener has not yet been started or has already been stopped");
            }

            _model.BasicCancel(_consumerTag);
            _model.Close();
            _started = false;
            _disposed = true;
            return Task.CompletedTask;
        }

        public IBasicPublishBatch CreateBasicPublishBatch()
        {
            return _model.CreateBasicPublishBatch();
        }

        internal void CreateHeadersAndRepublish(BasicDeliverEventArgs ea)
        {
            _model.BasicAck(ea.DeliveryTag, false);

            if (ea.BasicProperties.Headers == null)
            {
                ea.BasicProperties.Headers = new Dictionary<string, object>();
            }

            ea.BasicProperties.Headers["requeueCount"] = 0;
            //_logger.LogDebug("Republishing message");
            _model.BasicPublish(exchange: string.Empty, routingKey: ea.RoutingKey, basicProperties: ea.BasicProperties, body: ea.Body);
        }

        internal void RepublishMessages(BasicDeliverEventArgs ea)
        {
            int requeueCount = Convert.ToInt32(ea.BasicProperties.Headers["requeueCount"]);
            // Redelivered again
            requeueCount++;
            ea.BasicProperties.Headers["requeueCount"] = requeueCount;

            if (Convert.ToInt32(ea.BasicProperties.Headers["requeueCount"]) < 5)
            {
                _model.BasicAck(ea.DeliveryTag, false); // Manually ACK'ing, but resend
                //_logger.LogDebug("Republishing message");
                _model.BasicPublish(exchange: string.Empty, routingKey: ea.RoutingKey, basicProperties: ea.BasicProperties, body: ea.Body);
            }
            else
            {
                // Add message to dead letter exchange
                //_logger.LogDebug("Requeue count exceeded: rejecting message");
                _model.BasicReject(ea.DeliveryTag, false);
            }
        }

        internal Uri GenerateRabbitHostUri()
        {
            return new Uri($"amqp://{_attribute.Username}:{_attribute.Password}@{_attribute.HostName}:{_attribute.Port}");
        }

        public void Cancel()
        {
            StopAsync(CancellationToken.None).Wait();
        }

        public void Dispose()
        {

        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }
    }
}

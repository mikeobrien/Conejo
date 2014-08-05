using System;
using System.Text;
using Conejo.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Conejo
{
    public class Channel : IDisposable
    {
        private readonly ISerializer _serializer;
        private readonly Lazy<IModel> _channel;

        public Channel(
            Connection connection, 
            ChannelConfiguration configuration)
        {
            _serializer = connection.Configuration.Serializer;
            Configuration = configuration;
            _channel = new Lazy<IModel>(connection.CreateChannel);
        }

        public static Channel Create(
            Connection connection, 
            Action<ChannelConfigurationDsl> config)
        {
            return new Channel(connection, ChannelConfiguration.Create(config));
        }

        public static Channel Create(
            Connection connection, 
            ChannelConfiguration channelConfiguration, 
            Action<ChannelConfigurationDsl> config)
        {
            config(new ChannelConfigurationDsl(channelConfiguration));
            return new Channel(connection, channelConfiguration);
        }

        public ChannelConfiguration Configuration { get; private set; }

        public void Close()
        {
            _channel.Value.Close();
            _channel.Value.Dispose();
        }

        public void Dispose()
        {
            Close();
        }

        public Result Publish<TMessage>(TMessage message) 
            where TMessage : class, new()
        {
            try
            {
                EnsureExchangeAndQueue();

                _channel.Value.BasicPublish(
                    Configuration.ExchangeName,
                    BuildRoutingKey(message),
                    CreateBasicProperties(),
                    Encoding.UTF8.GetBytes(_serializer.Serialize(message)));
            }
            catch (Exception exception)
            {
                if (Configuration.ExceptionHandler == null) throw;
                Configuration.ExceptionHandler(exception);
                return new Result(exception);
            }
            return new Result();
        }

        public Result Subscribe<TMessage>(Action<TMessage> handler) 
            where TMessage : class, new()
        {
            try
            {
                EnsureExchangeAndQueue();

                var consumer = new EventingBasicConsumer(_channel.Value);

                consumer.Received += (c, result) =>
                {
                    if (Configuration.QueueAcknowledgeReciept)
                        _channel.Value.BasicAck(result.DeliveryTag, false);
                    handler(result.Body == null ? null : _serializer.Deserialize<TMessage>(
                        Encoding.UTF8.GetString(result.Body)));
                };

                _channel.Value.BasicConsume(Configuration.QueueName,
                    !Configuration.QueueAcknowledgeReciept, consumer);
            }
            catch (Exception exception)
            {
                if (Configuration.ExceptionHandler == null) throw;
                Configuration.ExceptionHandler(exception);
                return new Result(exception);
            }
            return new Result();
        }

        public Result<TMessage> Dequeue<TMessage>()
            where TMessage : class, new()
        {
            return Dequeue<TMessage>(x => x.Queue.Dequeue());
        }

        public Result<TMessage> Dequeue<TMessage>(bool wait)
            where TMessage : class, new()
        {
            return wait ? Dequeue<TMessage>() : 
                Dequeue<TMessage>(x => x.Queue.DequeueNoWait(
                    new BasicDeliverEventArgs()));
        }

        public Result<TMessage> Dequeue<TMessage>(int timeout)
            where TMessage : class, new()
        {
            return Dequeue<TMessage>(x =>
            {
                BasicDeliverEventArgs result;
                return x.Queue.Dequeue(timeout, out result) ? 
                    result : new BasicDeliverEventArgs();
            });
        }

        private Result<TMessage> Dequeue<TMessage>(Func<QueueingBasicConsumer, BasicDeliverEventArgs> dequeue)
            where TMessage : class, new()
        {
            try
            {
                EnsureExchangeAndQueue();

                var consumer = new QueueingBasicConsumer(_channel.Value);
                _channel.Value.BasicConsume(Configuration.QueueName, 
                    !Configuration.QueueAcknowledgeReciept, consumer);
                var result = dequeue(consumer);
                return new Result<TMessage>(result.Body == null ? null :
                    _serializer.Deserialize<TMessage>(Encoding.UTF8.GetString(result.Body)));
            }
            catch (Exception exception)
            {
                if (Configuration.ExceptionHandler == null) throw;
                Configuration.ExceptionHandler(exception);
                return new Result<TMessage>(exception);
            }
        }

        public Result Serve<TRequest, TResponse>(Func<TRequest, TResponse> handler)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            try
            {
                EnsureExchangeAndQueue();

                var consumer = new EventingBasicConsumer(_channel.Value);

                consumer.Received += (c, request) =>
                {
                    if (Configuration.QueueAcknowledgeReciept)
                        _channel.Value.BasicAck(request.DeliveryTag, false);

                    var response = handler(request.Body == null ? null : _serializer.Deserialize<TRequest>(
                        Encoding.UTF8.GetString(request.Body)));

                    var responseProperties = CreateBasicProperties();
                    responseProperties.CorrelationId = request.BasicProperties.CorrelationId;

                    _channel.Value.BasicPublish(
                        "",
                        request.BasicProperties.ReplyTo,
                        responseProperties,
                        Encoding.UTF8.GetBytes(_serializer.Serialize(response)));
                };

                _channel.Value.BasicConsume(Configuration.QueueName,
                    !Configuration.QueueAcknowledgeReciept, consumer);
            }
            catch (Exception exception)
            {
                if (Configuration.ExceptionHandler == null) throw;
                Configuration.ExceptionHandler(exception);
                return new Result(exception);
            }
            return new Result();
        }

        public Result<TResponse> Call<TRequest, TResponse>(TRequest message)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            return Call<TRequest, TResponse>(message, x => x.Queue.Dequeue());
        }

        public Result<TResponse> Call<TRequest, TResponse>(TRequest message, bool wait)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            return wait ? Dequeue<TResponse>() :
                Call<TRequest, TResponse>(message, x => x.Queue.DequeueNoWait(
                    new BasicDeliverEventArgs()));
        }

        public Result<TResponse> Call<TRequest, TResponse>(TRequest message, int timeout)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            return Call<TRequest, TResponse>(message, x =>
            {
                BasicDeliverEventArgs result;
                return x.Queue.Dequeue(timeout, out result) ?
                    result : new BasicDeliverEventArgs();
            });
        }

        private Result<TResponse> Call<TRequest, TResponse>(TRequest message, 
            Func<QueueingBasicConsumer, BasicDeliverEventArgs> dequeue)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            try
            {
                EnsureExchangeAndQueue();

                var requestProperties = CreateBasicProperties();
                requestProperties.CorrelationId = Guid.NewGuid().ToString();
                requestProperties.ReplyTo = _channel.Value.QueueDeclare(Guid.NewGuid().ToString(), false, true, true, null);

                _channel.Value.BasicPublish(
                    Configuration.ExchangeName,
                    BuildRoutingKey(message),
                    requestProperties,
                    Encoding.UTF8.GetBytes(_serializer.Serialize(message)));

                var consumer = new QueueingBasicConsumer(_channel.Value);
                _channel.Value.BasicConsume(requestProperties.ReplyTo,
                    !Configuration.QueueAcknowledgeReciept, consumer);

                while (true)
                {
                    var result = dequeue(consumer);
                    if (result.BasicProperties != null && 
                        result.BasicProperties.CorrelationId != 
                        requestProperties.CorrelationId) continue;
                    return result.Body == null ? new Result<TResponse>(new TimeoutException()) : 
                        new Result<TResponse>(_serializer.Deserialize<TResponse>(Encoding.UTF8.GetString(result.Body)));
                }
            }
            catch (Exception exception)
            {
                if (Configuration.ExceptionHandler == null) throw;
                Configuration.ExceptionHandler(exception);
                return new Result<TResponse>(exception);
            }
        }

        public Result Call<TRequest, TResponse>(TRequest message, Action<TResponse> handler)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            try
            {
                EnsureExchangeAndQueue();

                var requestProperties = CreateBasicProperties();
                requestProperties.CorrelationId = Guid.NewGuid().ToString();
                requestProperties.ReplyTo = _channel.Value.QueueDeclare(Guid.NewGuid().ToString(), false, true, true, null);

                _channel.Value.BasicPublish(
                    Configuration.ExchangeName,
                    BuildRoutingKey(message),
                    requestProperties,
                    Encoding.UTF8.GetBytes(_serializer.Serialize(message)));

                var consumer = new EventingBasicConsumer(_channel.Value);

                consumer.Received += (c, result) =>
                {
                    while (true)
                    {
                        if (result.BasicProperties != null &&
                            result.BasicProperties.CorrelationId !=
                            requestProperties.CorrelationId) continue;
                        handler(result.Body == null ? null : _serializer.Deserialize<TResponse>(
                            Encoding.UTF8.GetString(result.Body)));
                        if (Configuration.QueueAcknowledgeReciept)
                            _channel.Value.BasicAck(result.DeliveryTag, false);
                        break;
                    }
                };

                _channel.Value.BasicConsume(requestProperties.ReplyTo,
                    !Configuration.QueueAcknowledgeReciept, consumer);

                return new Result();
            }
            catch (Exception exception)
            {
                if (Configuration.ExceptionHandler == null) throw;
                Configuration.ExceptionHandler(exception);
                return new Result<TResponse>(exception);
            }
        }

        public void EnsureExchangeAndQueue()
        {
            EnsureExchange();
            EnsureQueue();
        }

        public void EnsureExchange()
        {
            _channel.Value.ExchangeDeclare(
                Configuration.ExchangeName.ToLower(),
                Configuration.ExchangeType.ToRabbitExchangeType(),
                Configuration.ExchangeDurable,
                Configuration.ExchangeAutoDelete, null);
        }

        public void EnsureQueue()
        {
            if (Configuration.QueueName.IsNullOrEmpty()) return;

            _channel.Value.QueueDeclare(
                Configuration.QueueName.ToLower(),
                Configuration.QueueDurable,
                Configuration.QueueExclusive,
                Configuration.QueueAutoDelete, null);

            _channel.Value.QueueBind(
                Configuration.QueueName.ToLower(),
                Configuration.ExchangeName.ToLower(),
                Configuration.QueueRoutingKey ?? "");
        }

        public void DeleteExchange()
        {
            _channel.Value.ExchangeDelete(Configuration.ExchangeName);
        }

        public void DeleteQueue()
        {
            _channel.Value.QueueDelete(Configuration.QueueName);
        }

        private string BuildRoutingKey<TMessage>(TMessage message)
        {
            return (Configuration.ExchangeType == ExchangeType.Topic ||
                    Configuration.ExchangeType == ExchangeType.Direct) && 
                    Configuration.ExchangeRoutingKey != null ? 
                        Configuration.ExchangeRoutingKey(message) ?? "" : "";
        }

        private IBasicProperties CreateBasicProperties()
        {
            var basicProperties = _channel.Value.CreateBasicProperties();
            basicProperties.ContentType = "application/json";
            basicProperties.ContentEncoding = "utf-8";
            return basicProperties;
        }
    }

    public class TimeoutException : Exception
    {
        public TimeoutException() : base(string.Format("RPC call timed out.")) { }
    }
}

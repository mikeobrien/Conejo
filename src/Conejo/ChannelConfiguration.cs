using System;
using System.Linq;

namespace Conejo
{
    public enum ExchangeType
    {
        Direct,
        Fanout,
        Headers,
        Topic
    }

    public class ChannelConfiguration
    {
        public static ChannelConfiguration Create(Action<ChannelConfigurationDsl> config)
        {
            var configuration = new ChannelConfiguration();
            config(new ChannelConfigurationDsl(configuration));
            return configuration;
        }

        public Action<Exception> ExceptionHandler { get; set; } 

        public string ExchangeName { get; set; }
        public ExchangeType ExchangeType { get; set; }
        public bool ExchangeDurable { get; set; }
        public bool ExchangeAutoDelete { get; set; }

        public string QueueName { get; set; }
        public Func<object, string> ExchangeTopic { get; set; }
        public string QueueTopic { get; set; }
        public bool QueueDurable { get; set; }
        public bool QueueExclusive { get; set; }
        public bool QueueAutoDelete { get; set; }
        public bool QueueAcknowledgeReciept { get; set; }
    }

    public class ChannelConfigurationDsl
    {
        private readonly ChannelConfiguration _configuration;

        public ChannelConfigurationDsl(ChannelConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ChannelConfigurationDsl WithExceptionHandler(Action<Exception> handler)
        {
            _configuration.ExceptionHandler = handler;
            return this;
        }

        public ExchangeConfigurationDsl ThroughDirectExchange()
        {
            return ThroughDirectExchange(Guid.NewGuid().ToString());
        }

        public ExchangeConfigurationDsl ThroughDirectExchange(string name)
        {
            _configuration.ExchangeName = name;
            _configuration.ExchangeType = ExchangeType.Direct;
            return new ExchangeConfigurationDsl(_configuration);
        }

        public ExchangeConfigurationDsl ThroughFanoutExchange()
        {
            return ThroughFanoutExchange(Guid.NewGuid().ToString());
        }

        public ExchangeConfigurationDsl ThroughFanoutExchange(string name)
        {
            _configuration.ExchangeName = name;
            _configuration.ExchangeType = ExchangeType.Fanout;
            return new ExchangeConfigurationDsl(_configuration);
        }

        public TopicExchangeConfigurationDsl ThroughTopicExchange()
        {
            return ThroughTopicExchange(Guid.NewGuid().ToString());
        }

        public TopicExchangeConfigurationDsl ThroughTopicExchange(string name)
        {
            _configuration.ExchangeName = name;
            _configuration.ExchangeType = ExchangeType.Topic;
            return new TopicExchangeConfigurationDsl(_configuration);
        }
    }

    public class ExchangeConfigurationDsl
    {
        private readonly ChannelConfiguration _channelConfiguration;

        public ExchangeConfigurationDsl(ChannelConfiguration channelConfiguration)
        {
            _channelConfiguration = channelConfiguration;
        }

        public ExchangeConfigurationDsl DurableExchange()
        {
            _channelConfiguration.ExchangeDurable = true;
            return this;
        }

        public ExchangeConfigurationDsl AutoDeleteExchange()
        {
            _channelConfiguration.ExchangeAutoDelete = true;
            return this;
        }

        public QueueConfigurationDsl InQueue()
        {
            return InQueue(Guid.NewGuid().ToString());
        }

        public QueueConfigurationDsl InQueue(string name)
        {
            _channelConfiguration.QueueName = name;
            return new QueueConfigurationDsl(_channelConfiguration);
        }
    }

    public class TopicExchangeConfigurationDsl
    {
        private readonly ChannelConfiguration _channelConfiguration;

        public TopicExchangeConfigurationDsl(ChannelConfiguration channelConfiguration)
        {
            _channelConfiguration = channelConfiguration;
        }

        public TopicExchangeConfigurationDsl DurableExchange()
        {
            _channelConfiguration.ExchangeDurable = true;
            return this;
        }

        public TopicExchangeConfigurationDsl AutoDeleteExchange()
        {
            _channelConfiguration.ExchangeAutoDelete = true;
            return this;
        }

        public TopicExchangeConfigurationDsl WithTopic(string topic)
        {
            _channelConfiguration.ExchangeTopic = message => topic;
            return this;
        }

        public TopicExchangeConfigurationDsl WithTopic<TMessage>(params Func<TMessage, string>[] topic)
        {
            _channelConfiguration.ExchangeTopic = message => topic.Select(x => x((TMessage)message)).Aggregate((a, i) => a + "." + i);
            return this;
        }

        public QueueConfigurationDsl InQueue(string topic)
        {
            return InQueue(Guid.NewGuid().ToString(), topic);
        }

        public QueueConfigurationDsl InQueue(string name, string topic)
        {
            _channelConfiguration.QueueName = name;
            _channelConfiguration.QueueTopic = topic;
            return new QueueConfigurationDsl(_channelConfiguration);
        }
    }

    public class QueueConfigurationDsl
    {
        private readonly ChannelConfiguration _channelConfiguration;

        public QueueConfigurationDsl(ChannelConfiguration channelConfiguration)
        {
            _channelConfiguration = channelConfiguration;
        }

        public QueueConfigurationDsl DurableQueue()
        {
            _channelConfiguration.QueueDurable = true;
            return this;
        }

        public QueueConfigurationDsl AutoDeleteQueue()
        {
            _channelConfiguration.QueueAutoDelete = true;
            return this;
        }

        public QueueConfigurationDsl ExclusiveQueue()
        {
            _channelConfiguration.QueueExclusive = true;
            return this;
        }

        public QueueConfigurationDsl ShouldAcknowledgeReciept()
        {
            _channelConfiguration.QueueAcknowledgeReciept = true;
            return this;
        }
    }
}

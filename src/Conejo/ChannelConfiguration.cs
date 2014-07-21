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
        public Func<object, string> ExchangeRoutingKey { get; set; }
        public string QueueRoutingKey { get; set; }
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

        public FanoutExchangeConfigurationDsl ThroughRandomFanoutExchange()
        {
            return ThroughFanoutExchange(Guid.NewGuid().ToString());
        }

        public FanoutExchangeConfigurationDsl ThroughFanoutExchange(string name)
        {
            _configuration.ExchangeName = name;
            _configuration.ExchangeType = ExchangeType.Fanout;
            return new FanoutExchangeConfigurationDsl(_configuration);
        }

        public DirectExchangeConfigurationDsl ThroughDefaultDirectExchange()
        {
            return ThroughDirectExchange("");
        }

        public DirectExchangeConfigurationDsl ThroughRandomDirectExchange()
        {
            return ThroughDirectExchange(Guid.NewGuid().ToString());
        }

        public DirectExchangeConfigurationDsl ThroughDirectExchange(string name)
        {
            _configuration.ExchangeName = name;
            _configuration.ExchangeType = ExchangeType.Direct;
            return new DirectExchangeConfigurationDsl(_configuration);
        }

        public TopicExchangeConfigurationDsl ThroughRandomTopicExchange()
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

    public class FanoutExchangeConfigurationDsl
    {
        private readonly ChannelConfiguration _channelConfiguration;

        public FanoutExchangeConfigurationDsl(ChannelConfiguration channelConfiguration)
        {
            _channelConfiguration = channelConfiguration;
        }

        public FanoutExchangeConfigurationDsl DurableExchange()
        {
            _channelConfiguration.ExchangeDurable = true;
            return this;
        }

        public FanoutExchangeConfigurationDsl AutoDeleteExchange()
        {
            _channelConfiguration.ExchangeAutoDelete = true;
            return this;
        }

        public FanoutQueueConfigurationDsl InRandomQueue()
        {
            return InQueue(Guid.NewGuid().ToString());
        }

        public FanoutQueueConfigurationDsl InQueue(string name)
        {
            _channelConfiguration.QueueName = name;
            return new FanoutQueueConfigurationDsl(_channelConfiguration);
        }
    }

    public class FanoutQueueConfigurationDsl
    {
        private readonly ChannelConfiguration _channelConfiguration;

        public FanoutQueueConfigurationDsl(ChannelConfiguration channelConfiguration)
        {
            _channelConfiguration = channelConfiguration;
        }

        public FanoutQueueConfigurationDsl DurableQueue()
        {
            _channelConfiguration.QueueDurable = true;
            return this;
        }

        public FanoutQueueConfigurationDsl AutoDeleteQueue()
        {
            _channelConfiguration.QueueAutoDelete = true;
            return this;
        }

        public FanoutQueueConfigurationDsl ExclusiveQueue()
        {
            _channelConfiguration.QueueExclusive = true;
            return this;
        }

        public FanoutQueueConfigurationDsl ShouldAcknowledgeReciept()
        {
            _channelConfiguration.QueueAcknowledgeReciept = true;
            return this;
        }
    }

    public class DirectExchangeConfigurationDsl
    {
        private readonly ChannelConfiguration _channelConfiguration;

        public DirectExchangeConfigurationDsl(ChannelConfiguration channelConfiguration)
        {
            _channelConfiguration = channelConfiguration;
        }

        public DirectExchangeConfigurationDsl DurableExchange()
        {
            _channelConfiguration.ExchangeDurable = true;
            return this;
        }

        public DirectExchangeConfigurationDsl AutoDeleteExchange()
        {
            _channelConfiguration.ExchangeAutoDelete = true;
            return this;
        }

        public DirectExchangeConfigurationDsl WithRoutingKey(string routingKey)
        {
            _channelConfiguration.ExchangeRoutingKey = message => routingKey;
            return this;
        }

        public DirectQueueConfigurationDsl InRandomQueue()
        {
            return InQueue(Guid.NewGuid().ToString());
        }

        public DirectQueueConfigurationDsl InQueue(string name)
        {
            _channelConfiguration.QueueName = name;
            return new DirectQueueConfigurationDsl(_channelConfiguration);
        }
    }

    public class DirectQueueConfigurationDsl
    {
        private readonly ChannelConfiguration _channelConfiguration;

        public DirectQueueConfigurationDsl(ChannelConfiguration channelConfiguration)
        {
            _channelConfiguration = channelConfiguration;
        }

        public DirectQueueConfigurationDsl DurableQueue()
        {
            _channelConfiguration.QueueDurable = true;
            return this;
        }

        public DirectQueueConfigurationDsl AutoDeleteQueue()
        {
            _channelConfiguration.QueueAutoDelete = true;
            return this;
        }

        public DirectQueueConfigurationDsl ExclusiveQueue()
        {
            _channelConfiguration.QueueExclusive = true;
            return this;
        }

        public DirectQueueConfigurationDsl ShouldAcknowledgeReciept()
        {
            _channelConfiguration.QueueAcknowledgeReciept = true;
            return this;
        }

        public DirectQueueConfigurationDsl WithRoutingKey(string routingKey)
        {
            _channelConfiguration.QueueRoutingKey = routingKey;
            return this;
        }

        public DirectQueueConfigurationDsl WithRoutingKeyAsQueueName()
        {
            _channelConfiguration.QueueRoutingKey = _channelConfiguration.QueueName;
            return this;
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
            _channelConfiguration.ExchangeRoutingKey = message => topic;
            return this;
        }

        public TopicExchangeConfigurationDsl WithTopic<TMessage>(params Func<TMessage, string>[] topic)
        {
            _channelConfiguration.ExchangeRoutingKey = message => topic.Select(x => x((TMessage)message)).Aggregate((a, i) => a + "." + i);
            return this;
        }

        public TopicQueueConfigurationDsl InRandomQueue()
        {
            return InQueue(Guid.NewGuid().ToString());
        }

        public TopicQueueConfigurationDsl InQueue(string name)
        {
            _channelConfiguration.QueueName = name;
            return new TopicQueueConfigurationDsl(_channelConfiguration);
        }
    }

    public class TopicQueueConfigurationDsl
    {
        private readonly ChannelConfiguration _channelConfiguration;

        public TopicQueueConfigurationDsl(ChannelConfiguration channelConfiguration)
        {
            _channelConfiguration = channelConfiguration;
        }

        public TopicQueueConfigurationDsl DurableQueue()
        {
            _channelConfiguration.QueueDurable = true;
            return this;
        }

        public TopicQueueConfigurationDsl AutoDeleteQueue()
        {
            _channelConfiguration.QueueAutoDelete = true;
            return this;
        }

        public TopicQueueConfigurationDsl ExclusiveQueue()
        {
            _channelConfiguration.QueueExclusive = true;
            return this;
        }

        public TopicQueueConfigurationDsl ShouldAcknowledgeReciept()
        {
            _channelConfiguration.QueueAcknowledgeReciept = true;
            return this;
        }

        public TopicQueueConfigurationDsl WithTopic(string topic)
        {
            _channelConfiguration.QueueRoutingKey = topic;
            return this;
        }
    }
}

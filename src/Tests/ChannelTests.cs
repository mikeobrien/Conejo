using System;
using System.Threading;
using Conejo;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class ChannelTests
    {
        public class Message
        {
            public string Text { get; set; }
        }

        private const int Timeout = 1000;
        private readonly Message _publishMessage = new Message { Text = "hai" };
        private Connection _connection;

        [SetUp]
        public void Setup()
        {
            _connection = Connection.Create();
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Close();
        }

        [Test]
        public void should_publish_and_subscribe_through_default_queue()
        {
            var routingKey = Guid.NewGuid().ToString();
            var publisher = Channel.Create(_connection, x => x
                .ThroughDefaultDirectExchange()
                    .WithRoutingKey(routingKey));

            var subscriber = Channel.Create(_connection, x => x
                .ThroughDefaultDirectExchange()
                .InQueue(routingKey));

            subscriber.EnsureQueue();

            publisher.Publish(_publishMessage);
            var response = subscriber.Dequeue<Message>(Timeout);

            response.ShouldNotBeNull();
            response.Message.Text.ShouldEqual(_publishMessage.Text);

            subscriber.DeleteQueue();
            subscriber.Close();

            publisher.Close();
        }

        [Test]
        public void should_publish_and_subscribe_through_direct_queue()
        {
            var publisher = Channel.Create(_connection, x => x
                .ThroughRandomDirectExchange());

            var subscriber = Channel.Create(_connection, x => x
                .ThroughDirectExchange(publisher.Configuration.ExchangeName)
                .InRandomQueue());

            publisher.EnsureExchange();
            subscriber.EnsureQueue();

            publisher.Publish(_publishMessage);
            var response = subscriber.Dequeue<Message>(Timeout);

            response.ShouldNotBeNull();
            response.Message.Text.ShouldEqual(_publishMessage.Text);

            subscriber.DeleteQueue();
            subscriber.Close();

            publisher.DeleteExchange();
            publisher.Close();
        }

        [Test]
        public void should_publish_and_subscribe_through_direct_queue_async()
        {
            var publisher = Channel.Create(_connection, x => x
                .ThroughRandomDirectExchange());

            var subscriber = Channel.Create(_connection, x => x
                .ThroughDirectExchange(publisher.Configuration.ExchangeName)
                .InRandomQueue());

            publisher.EnsureExchange();
            subscriber.EnsureQueue();

            publisher.Publish(_publishMessage);

            Message responseMessage = null;
            var response = subscriber.Subscribe<Message>(x => responseMessage = x);

            response.ShouldNotBeNull();
            response.Error.ShouldBeFalse();

            Thread.Sleep(Timeout);

            responseMessage.ShouldNotBeNull();
            responseMessage.Text.ShouldEqual(_publishMessage.Text);

            subscriber.DeleteQueue();
            subscriber.Close();

            publisher.DeleteExchange();
            publisher.Close();
        }

        [Test]
        public void should_publish_and_subscribe_through_topic_queue()
        {
            var publisher = Channel.Create(_connection, x => x
                .ThroughRandomTopicExchange()
                .WithTopic<Message>(y => "oh", y => y.Text));

            var subscriberMatched = Channel.Create(_connection, x => x
                .ThroughTopicExchange(publisher.Configuration.ExchangeName)
                .InRandomQueue()
                    .WithTopic("*.hai"));

            var subscriberNotMatched = Channel.Create(_connection, x => x
                .ThroughTopicExchange(publisher.Configuration.ExchangeName)
                .InRandomQueue()
                    .WithTopic("yada.*"));

            publisher.EnsureExchange();
            subscriberNotMatched.EnsureQueue();
            subscriberMatched.EnsureQueue();

            publisher.Publish(_publishMessage);

            var notMatchedResponse = subscriberNotMatched.Dequeue<Message>(Timeout);

            notMatchedResponse.ShouldNotBeNull();
            notMatchedResponse.Message.ShouldBeNull();

            var matchedResponse = subscriberMatched.Dequeue<Message>(Timeout);

            matchedResponse.ShouldNotBeNull();
            matchedResponse.Message.Text.ShouldEqual(_publishMessage.Text);

            subscriberMatched.DeleteQueue();
            subscriberMatched.Close();

            subscriberNotMatched.DeleteQueue();
            subscriberNotMatched.Close();

            publisher.DeleteExchange();
            publisher.Close();
        }

        [Test]
        public void should_publish_and_subscribe_through_fanout_queue()
        {
            var publisher = Channel.Create(_connection, x => x
                .ThroughRandomFanoutExchange());

            var subscriber1 = Channel.Create(_connection, x => x
                .ThroughFanoutExchange(publisher.Configuration.ExchangeName)
                .InRandomQueue());

            var subscriber2 = Channel.Create(_connection, x => x
                .ThroughFanoutExchange(publisher.Configuration.ExchangeName)
                .InRandomQueue());

            publisher.EnsureExchange();
            subscriber2.EnsureQueue();
            subscriber1.EnsureQueue();

            publisher.Publish(_publishMessage);

            var response1 = subscriber1.Dequeue<Message>(Timeout);

            response1.ShouldNotBeNull();
            response1.Message.Text.ShouldEqual(_publishMessage.Text);

            var response2 = subscriber2.Dequeue<Message>(Timeout);

            response2.ShouldNotBeNull();
            response2.Message.Text.ShouldEqual(_publishMessage.Text);

            subscriber1.DeleteQueue();
            subscriber1.Close();

            subscriber2.DeleteQueue();
            subscriber2.Close();

            publisher.DeleteExchange();
            publisher.Close();
        }

        [Test]
        public void should_rpc()
        {
            var server = Channel.Create(_connection, x => x
                .ThroughRandomDirectExchange()
                .InRandomQueue()
                    .WithRoutingKeyAsQueueName());

            var client = Channel.Create(_connection, x => x
                .ThroughDirectExchange(server.Configuration.ExchangeName)
                .WithRoutingKey(server.Configuration.QueueName));

            server.EnsureExchange();
            server.EnsureQueue();

            server.Serve<Message, Message>(x => x);

            var response = client.Call<Message, Message>(_publishMessage, Timeout);

            response.ShouldNotBeNull();
            response.Error.ShouldBeFalse();
            response.Message.ShouldNotBeNull();
            response.Message.Text.ShouldEqual(_publishMessage.Text);

            client.Close();

            server.DeleteQueue();
            server.DeleteExchange();
            server.Close();
        }

        [Test]
        public void should_rpc_async()
        {
            var server = Channel.Create(_connection, x => x
                .ThroughRandomDirectExchange()
                .InRandomQueue());

            var client = Channel.Create(_connection, x => x
                .ThroughDirectExchange(server.Configuration.ExchangeName)
                .InQueue(server.Configuration.QueueName));

            server.EnsureExchange();
            server.EnsureQueue();

            server.Serve<Message, Message>(x => x);

            Message responseMessage = null;

            var response = client.Call<Message, Message>(_publishMessage, x => responseMessage = x);

            response.ShouldNotBeNull();
            response.Error.ShouldBeFalse();

            Thread.Sleep(Timeout);

            responseMessage.ShouldNotBeNull();
            responseMessage.Text.ShouldEqual(_publishMessage.Text);

            client.Close();

            server.DeleteQueue();
            server.DeleteExchange();
            server.Close();
        }
    }
}

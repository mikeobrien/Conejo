using System;
using System.Threading;
using Conejo;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class DefinitionTests
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
            _connection.Dispose();
        }

        private static readonly string PubSubDirectExchange = Guid.NewGuid().ToString();

        public class TestDirectPublisher : PublisherDefinition<Message>
        {
            public TestDirectPublisher(Connection connection) : base(connection)
            {
                Configure(x => x.ThroughDirectExchange(PubSubDirectExchange));
            }
        }

        public class TestDirectSubscriber : SubscriberDefinition<Message>
        {
            public TestDirectSubscriber(Connection connection) : base(connection)
            {
                Configure(x => x
                    .ThroughDirectExchange(PubSubDirectExchange)
                    .InRandomQueue());
            }
        }

        [Test]
        public void should_publish_and_subscribe_through_direct_queue()
        {
            var publisher = new TestDirectPublisher(_connection);
            var subscriber = new TestDirectSubscriber(_connection);

            publisher.Channel.EnsureExchange();
            subscriber.Channel.EnsureQueue();

            publisher.Publish(_publishMessage);
            var response = subscriber.Dequeue(Timeout);

            response.ShouldNotBeNull();
            response.Message.Text.ShouldEqual(_publishMessage.Text);

            subscriber.Channel.DeleteQueue();
            subscriber.Close();

            publisher.Channel.DeleteExchange();
            publisher.Close();
        }

        [Test]
        public void should_publish_and_subscribe_through_direct_queue_async()
        {
            var publisher = new TestDirectPublisher(_connection);
            var subscriber = new TestDirectSubscriber(_connection);

            publisher.Channel.EnsureExchange();
            subscriber.Channel.EnsureQueue();

            publisher.Publish(_publishMessage);

            Message responseMessage = null;
            var response = subscriber.Subscribe(x => responseMessage = x);

            response.ShouldNotBeNull();
            response.Error.ShouldBeFalse();

            Thread.Sleep(Timeout);

            responseMessage.ShouldNotBeNull();
            responseMessage.Text.ShouldEqual(_publishMessage.Text);

            subscriber.Channel.DeleteQueue();
            subscriber.Close();

            publisher.Channel.DeleteExchange();
            publisher.Close();
        }

        private static readonly string PubSubTopicExchange = Guid.NewGuid().ToString();

        public class TestTopicPublisher : PublisherDefinition<Message>
        {
            public TestTopicPublisher(Connection connection) : base(connection)
            {
                Configure(x => x
                    .ThroughTopicExchange(PubSubTopicExchange)
                    .WithTopic<Message>(y => "oh", y => y.Text));
            }
        }

        public class TestTopicMatchedSubscriber : SubscriberDefinition<Message>
        {
            public TestTopicMatchedSubscriber(Connection connection) : base(connection)
            {
                Configure(x => x
                    .ThroughTopicExchange(PubSubTopicExchange)
                    .InRandomQueue()
                        .WithTopic("*.hai"));
            }
        }

        public class TestTopicNotMatchedSubscriber : SubscriberDefinition<Message>
        {
            public TestTopicNotMatchedSubscriber(Connection connection) : base(connection)
            {
                Configure(x => x
                    .ThroughTopicExchange(PubSubTopicExchange)
                    .InRandomQueue()
                        .WithTopic("yada.*"));
            }
        }

        [Test]
        public void should_publish_and_subscribe_through_topic_queue()
        {
            var publisher = new TestTopicPublisher(_connection);
            var subscriberMatched = new TestTopicMatchedSubscriber(_connection);
            var subscriberNotMatched = new TestTopicNotMatchedSubscriber(_connection);

            publisher.Channel.EnsureExchange();
            subscriberNotMatched.Channel.EnsureQueue();
            subscriberMatched.Channel.EnsureQueue();

            publisher.Publish(_publishMessage);

            var notMatchedResponse = subscriberNotMatched.Dequeue(Timeout);

            notMatchedResponse.ShouldNotBeNull();
            notMatchedResponse.Message.ShouldBeNull();

            var matchedResponse = subscriberMatched.Dequeue(Timeout);

            matchedResponse.ShouldNotBeNull();
            matchedResponse.Message.Text.ShouldEqual(_publishMessage.Text);

            subscriberMatched.Channel.DeleteQueue();
            subscriberMatched.Close();

            subscriberNotMatched.Channel.DeleteQueue();
            subscriberNotMatched.Close();

            publisher.Channel.DeleteExchange();
            publisher.Close();
        }

        private static readonly string PubSubFanoutExchange = Guid.NewGuid().ToString();

        public class TestFanoutPublisher : PublisherDefinition<Message>
        {
            public TestFanoutPublisher(Connection connection) : base(connection)
            {
                Configure(x => x.ThroughFanoutExchange(PubSubFanoutExchange));
            }
        }

        public class TestFanoutSubscriber1 : SubscriberDefinition<Message>
        {
            public TestFanoutSubscriber1(Connection connection) : base(connection)
            {
                Configure(x => x
                    .ThroughFanoutExchange(PubSubFanoutExchange)
                    .InRandomQueue());
            }
        }

        public class TestFanoutSubscriber2 : SubscriberDefinition<Message>
        {
            public TestFanoutSubscriber2(Connection connection) : base(connection)
            {
                Configure(x => x
                    .ThroughFanoutExchange(PubSubFanoutExchange)
                    .InRandomQueue());
            }
        }

        [Test]
        public void should_publish_and_subscribe_through_fanout_queue()
        {
            var publisher = new TestFanoutPublisher(_connection);
            var subscriber1 = new TestFanoutSubscriber1(_connection);
            var subscriber2 = new TestFanoutSubscriber2(_connection);

            publisher.Channel.EnsureExchange();
            subscriber2.Channel.EnsureQueue();
            subscriber1.Channel.EnsureQueue();

            publisher.Publish(_publishMessage);

            var response1 = subscriber1.Dequeue(Timeout);

            response1.ShouldNotBeNull();
            response1.Message.Text.ShouldEqual(_publishMessage.Text);

            var response2 = subscriber2.Dequeue(Timeout);

            response2.ShouldNotBeNull();
            response2.Message.Text.ShouldEqual(_publishMessage.Text);

            subscriber1.Channel.DeleteQueue();
            subscriber1.Close();

            subscriber2.Channel.DeleteQueue();
            subscriber2.Close();

            publisher.Channel.DeleteExchange();
            publisher.Close();
        }

        private static readonly string RpcExchange = Guid.NewGuid().ToString();
        private static readonly string RpcQueue = Guid.NewGuid().ToString();

        public class TestRpcServer : RpcServerDefinition<Message, Message>
        {
            public TestRpcServer(Connection connection) : base(connection)
            {
                Configure(x => x
                    .ThroughDirectExchange(RpcExchange)
                    .InQueue(RpcQueue)
                        .WithRoutingKeyAsQueueName());
            }
        }

        public class TestRpcClient : RpcClientDefinition<Message, Message>
        {
            public TestRpcClient(Connection connection) : base(connection)
            {
                Configure(x => x
                    .ThroughDirectExchange(RpcExchange)
                    .WithRoutingKey(RpcQueue));
            }
        }

        [Test]
        public void should_rpc()
        {
            var server = new TestRpcServer(_connection);
            var client = new TestRpcClient(_connection);

            server.Channel.EnsureExchange();
            server.Channel.EnsureQueue();

            server.Serve(x => x);

            var response = client.Call(_publishMessage, Timeout);

            response.ShouldNotBeNull();
            response.Error.ShouldBeFalse();
            response.Message.ShouldNotBeNull();
            response.Message.Text.ShouldEqual(_publishMessage.Text);

            client.Close();

            server.Channel.DeleteQueue();
            server.Channel.DeleteExchange();
            server.Close();
        }

        [Test]
        public void should_rpc_async()
        {
            var server = new TestRpcServer(_connection);
            var client = new TestRpcClient(_connection);

            server.Channel.EnsureExchange();
            server.Channel.EnsureQueue();

            server.Serve(x => x);

            Message responseMessage = null;

            var response = client.Call(_publishMessage, x => responseMessage = x);

            response.ShouldNotBeNull();
            response.Error.ShouldBeFalse();

            Thread.Sleep(Timeout);

            responseMessage.ShouldNotBeNull();
            responseMessage.Text.ShouldEqual(_publishMessage.Text);

            client.Close();

            server.Channel.DeleteQueue();
            server.Channel.DeleteExchange();
            server.Close();
        }
    }
}


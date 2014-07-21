Conejo
=============

<img src="https://raw.github.com/mikeobrien/Conejo/master/misc/logo.png"/>

Conejo is a friendly wrapper around the [Pivotal RabbitMQ client](https://www.rabbitmq.com/dotnet.html). It provides fluent configuration and DI friendly exchange and queue definitions in the spirit of [Fluent NHibernate](http://www.fluentnhibernate.org/).

Install
------------

Conejo can be found on nuget:

    PM> Install-Package Conejo

Basic Pub/sub Usage
------------

The following demonstrates basic pub/sub usage:

```csharp
public class Message
{
    public string Text { get; set; }
}

var connection =
    Connection.Create(x => x
        .ConnectTo("host", "virtual host")
        .WithCredentials("guest", "guest"));

var publisher =
    Channel.Create(connection, x => x
        .ThroughTopicExchange("pubsub")
            .ThatsDurable()
            .WithTopic("oh.hai"));

var subscriber =
    Channel.Create(connection, x => x
        .ThroughTopicExchange("pubsub")
            .ThatsDurable()
            .InQueue("messages")
                .WithTopic("*.hai")
                .ThatsExclusive()
                .ThatsAutoDeleted());

publisher.EnsureExchange();
subscriber.EnsureQueue();

publisher.Publish(new Message { Text = "hai" });

var response = subscriber.Dequeue<Message>();

response.Message.Text.ShouldEqual("hai");
```

The API also supports passing a lambda:

```csharp
subscriber.Subscribe<Message>(x => Console.WriteLine(x.Text));
```

Basic RPC Usage
------------

The following demonstrates basic RPC usage:

```csharp
public class Request
{
    public string Text { get; set; }
}

public class Response
{
    public string Text { get; set; }
}

var server = 
    Channel.Create(_connection, x => x
        .ThroughDirectExchange("rpc")
            .InQueue("ping")
                .WithRoutingKeyAsQueueName());

var client = 
    Channel.Create(_connection, x => x
        .ThroughDirectExchange("rpc")
            .WithRoutingKey("ping"));

server.EnsureExchangeAndQueue();

server.Subscribe<Request, Response>(x => new Response { Text = x.Text });

var response = client.Call<Request, Response>(new Request { Text = "hai" });

response.Message.Text.ShouldEqual("hai");
```

The API also supports passing a lambda:

```csharp
client.Call<Request, Response>(x => Console.WriteLine(x.Text));
```

Using Definitions
------------

Definitions allow you to define an exchange/queue in much the same way you define Fluent NHibernate mappings. For example the definitions for the pub/sub example above would be as follows:

```csharp
public class Publisher : PublisherDefinition<Message>
{
    public Publisher(Connection connection) : base(connection)
    {
        Configure(x => x
            .ThroughTopicExchange("pubsub")
                .ThatsDurable()
                .WithTopic<Message>("oh.hai"));
    }
}

public class Subscriber : SubscriberDefinition<Message>
{
    public Subscriber(Connection connection) : base(connection)
    {
        Configure(x => x
            .ThroughTopicExchange("pubsub")
                .ThatsDurable()
                .InQueue("messages")
                    .WithTopic("*.hai")
                    .ThatsExclusive()
                    .ThatsAutoDeleted());
    }
}
```

These can then be registered in your IoC container, for example StructureMap:

```csharp
ObjectFactory.Configure(x =>
{
    For<Connection>().Use(() => 
        Connection.Create(x => x
            .ConnectTo("host", "virtual host")
            .WithCredentials("guest, "guest"));

    Scan(x =>
    {
        x.TheCallingAssembly(); 
        x.AddAllTypesOf(typeof(IPublisher<>));
        x.AddAllTypesOf(typeof(ISubscriber<>));
    });
});
```

And taken as a dependency:

```csharp
public class PubsubService
{
    public PubsubService(
        IPublisher<TMessage> publisher,
        ISubscriber<TMessage> subscriber)
    { ... }
}
```

There are 4 types of definitions corresponding to four types of basic functionality: `IPublisher<TMessage>`, `ISubscriber<TMessage>`, `IRpcServer<TRequest, TResponse>`, `IRpcClient<TRequest, TResponse>`.

Props
------------

Thanks to [JetBrains](http://www.jetbrains.com/) for providing OSS licenses!

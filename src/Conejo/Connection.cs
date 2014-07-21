using System;
using Conejo.Extensions;
using RabbitMQ.Client;

namespace Conejo
{
    public class Connection : IDisposable
    {
        private readonly Lazy<IConnection> _connection;

        public Connection(ConnectionConfiguration configuration)
        {
            _connection = new Lazy<IConnection>(() => CreateConnectionFactory(configuration).CreateConnection());
            Configuration = configuration;
        }

        public static Connection Create()
        {
            return new Connection(new ConnectionConfiguration());
        }

        public static Connection Create(Action<ConnectionConfigurationDsl> config)
        {
            return new Connection(ConnectionConfiguration.Create(config));
        }

        public static Connection Create(ConnectionConfiguration channelConfiguration, Action<ConnectionConfigurationDsl> config)
        {
            config(new ConnectionConfigurationDsl(channelConfiguration));
            return new Connection(channelConfiguration);
        }

        public ConnectionConfiguration Configuration { get; private set; }

        public IModel CreateChannel()
        {
            return _connection.Value.CreateModel();
        }

        public void Close()
        {
            if (!_connection.IsValueCreated) return;
            _connection.Value.Close();
            _connection.Value.Dispose();
        }

        public void Dispose()
        {
            Close();
        }

        private static ConnectionFactory CreateConnectionFactory(ConnectionConfiguration configuration)
        {
            var connectionFactory = new ConnectionFactory();
            if (configuration.Host.IsNotNullOrEmpty())
                connectionFactory.HostName = configuration.Host;
            if (configuration.Port.HasValue)
                connectionFactory.Port = configuration.Port.Value;
            if (configuration.VirtualHost.IsNotNullOrEmpty())
                connectionFactory.VirtualHost = configuration.VirtualHost;
            if (configuration.Username.IsNotNullOrEmpty())
                connectionFactory.UserName = configuration.Username;
            if (configuration.Password.IsNotNullOrEmpty())
                connectionFactory.Password = configuration.Password;
            if (configuration.Uri.IsNotNullOrEmpty())
                connectionFactory.Uri = configuration.Uri;
            return connectionFactory;
        }
    }
}

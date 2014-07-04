using System;

namespace Conejo
{
    public class ConnectionConfiguration
    {
        public static ConnectionConfiguration Create(Action<ConnectionConfigurationDsl> config)
        {
            var configuration = new ConnectionConfiguration();
            config(new ConnectionConfigurationDsl(configuration));
            return configuration;
        }

        public string Uri { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
    }

    public class ConnectionConfigurationDsl
    {
        private readonly ConnectionConfiguration _configuration;

        public ConnectionConfigurationDsl(ConnectionConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ConnectionConfigurationDsl ConnectToUri(string uri)
        {
            _configuration.Uri = uri;
            return this;
        }

        public ConnectionConfigurationDsl ConnectTo(string host, string virtualHost = null, int? port = null)
        {
            _configuration.Host = host;
            _configuration.VirtualHost = virtualHost;
            _configuration.Port = port;
            return this;
        }

        public ConnectionConfigurationDsl WithCredentials(string username, string password)
        {
            _configuration.Username = username;
            _configuration.Password = password;
            return this;
        }
    }
}

using System;

namespace Conejo
{
    public abstract class DefinitionBase : IDisposable
    {
        protected DefinitionBase(Connection connection)
        {
            Configuration = new ChannelConfiguration();
            Channel = new Channel(connection, Configuration);
        }

        protected void Configure(Action<ChannelConfigurationDsl> config)
        {
            config(new ChannelConfigurationDsl(Configuration));
        }

        public ChannelConfiguration Configuration { get; private set; }
        public Channel Channel { get; private set; }

        public void Close()
        {
            Channel.Close();
        }

        public void Dispose()
        {
            Close();
        }
    }
}

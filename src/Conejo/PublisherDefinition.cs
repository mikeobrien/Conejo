namespace Conejo
{
    public abstract class PublisherDefinition<TMessage> : 
        DefinitionBase, IPublisher<TMessage>
        where TMessage : class, new()
    {
        protected PublisherDefinition(Connection connection) : base(connection) { }

        public virtual Result Publish(TMessage message)
        {
            return Channel.Publish(message);
        }
    }
}

using System;

namespace Conejo
{
    public abstract class SubscriberDefinition<TMessage> :
        DefinitionBase, ISubscriber<TMessage>
        where TMessage : class, new()
    {
        protected SubscriberDefinition(Connection connection) : base(connection) { }

        public virtual Result<TMessage> Dequeue()
        {
            return Channel.Dequeue<TMessage>();
        }

        public virtual Result<TMessage> Dequeue(bool wait)
        {
            return Channel.Dequeue<TMessage>(wait);
        }

        public virtual Result<TMessage> Dequeue(int timeout)
        {
            return Channel.Dequeue<TMessage>(timeout);
        }

        public virtual Result Subscribe(Action<TMessage> handler)
        {
            return Channel.Subscribe(handler);
        }
    }
}

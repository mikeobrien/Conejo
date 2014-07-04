using System;

namespace Conejo
{
    public interface ISubscriber<TMessage> where TMessage : class, new()
    {
        Result<TMessage> Dequeue();
        Result<TMessage> Dequeue(bool wait);
        Result<TMessage> Dequeue(int timeout);
        Result Subscribe(Action<TMessage> handler);
    }
}

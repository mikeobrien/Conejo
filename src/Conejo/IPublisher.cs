namespace Conejo
{
    public interface IPublisher<TMessage> where TMessage : class, new()
    {
        Result Publish(TMessage message);
    }
}

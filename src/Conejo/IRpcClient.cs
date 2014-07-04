using System;

namespace Conejo
{
    public interface IRpcClient<TRequest, TResponse>
        where TRequest : class, new()
        where TResponse : class, new()
    {
        Result<TResponse> Call(TRequest message);
        Result<TResponse> Call(TRequest message, bool wait);
        Result<TResponse> Call(TRequest message, int timeout);
        Result Call(TRequest message, Action<TResponse> handler);
    }
}

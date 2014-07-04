using System;

namespace Conejo
{
    public interface IRpcServer<TRequest, TResponse>
        where TRequest : class, new()
        where TResponse : class, new()
    {
        Result Subscribe(Func<TRequest, TResponse> handler);
    }
}

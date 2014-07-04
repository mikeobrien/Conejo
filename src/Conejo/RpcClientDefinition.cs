using System;

namespace Conejo
{
    public abstract class RpcClientDefinition<TRequest, TResponse> :
        DefinitionBase, IRpcClient<TRequest, TResponse>
        where TRequest : class, new()
        where TResponse : class, new()
    {
        protected RpcClientDefinition(Connection connection) : base(connection) { }

        public virtual Result<TResponse> Call(TRequest message)
        {
            return Channel.Call<TRequest, TResponse>(message);
        }

        public virtual Result<TResponse> Call(TRequest message, bool wait)
        {
            return Channel.Call<TRequest, TResponse>(message, wait);
        }

        public virtual Result<TResponse> Call(TRequest message, int timeout)
        {
            return Channel.Call<TRequest, TResponse>(message, timeout);
        }

        public virtual Result Call(TRequest message, Action<TResponse> handler)
        {
            return Channel.Call(message, handler);
        }
    }
}

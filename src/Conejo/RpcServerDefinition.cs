using System;

namespace Conejo
{
    public abstract class RpcServerDefinition<TRequest, TResponse> :
        DefinitionBase, IRpcServer<TRequest, TResponse>
        where TRequest : class, new()
        where TResponse : class, new()
    {
        protected RpcServerDefinition(Connection connection) : base(connection) { }

        public virtual Result Serve(Func<TRequest, TResponse> handler)
        {
            return Channel.Serve(handler);
        }
    }
}

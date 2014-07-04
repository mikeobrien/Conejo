using System;

namespace Conejo
{
    public class Result
    {
        public Result() { }

        public Result(Exception exception)
        {
            Error = true;
            Exception = exception;
        }

        public bool Error { get; private set; }
        public Exception Exception { get; private set; }
    }

    public class Result<TMessage> : Result
    {
        public Result(TMessage message)
        {
            Message = message;
        }

        public Result(Exception exception) : base(exception) { }

        public TMessage Message { get; private set; }
    }
}

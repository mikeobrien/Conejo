using System;

namespace Conejo.Extensions
{
    public static class Extensions
    {
        public static string ToRabbitExchangeType(this ExchangeType type)
        {
            switch (type)
            {
                case ExchangeType.Direct: return RabbitMQ.Client.ExchangeType.Direct;
                case ExchangeType.Fanout: return RabbitMQ.Client.ExchangeType.Fanout;
                case ExchangeType.Headers: return RabbitMQ.Client.ExchangeType.Headers;
                case ExchangeType.Topic: return RabbitMQ.Client.ExchangeType.Topic;
            }
            throw new Exception();
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !value.IsNullOrEmpty();
        }
    }
}

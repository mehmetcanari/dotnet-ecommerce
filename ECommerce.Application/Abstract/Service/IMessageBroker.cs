public interface IMessageBroker
{
    Task PublishAsync<T>(T message, string exchange, string routingKey);
}
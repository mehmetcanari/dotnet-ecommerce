public interface IMessageBroker
{
    Task PublishAsync<T>(T message, string exchange, string routingKey);
    Task SubscribeAsync<T>(string queue, Func<T, Task> handler);
}
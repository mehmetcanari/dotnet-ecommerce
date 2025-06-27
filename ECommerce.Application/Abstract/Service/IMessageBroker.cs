namespace ECommerce.Application.Abstract.Service;

public interface IMessageBroker
{
    Task PublishAsync<T>(T message, string exchange, string routingKey) where T : class;
}
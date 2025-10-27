namespace ECommerce.Application.Abstract;

public interface IMessageBroker
{
    Task Publish<T>(T message, string exchange, string routingKey) where T : class;
}
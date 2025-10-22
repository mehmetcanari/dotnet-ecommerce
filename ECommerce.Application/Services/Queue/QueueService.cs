using System.Text;
using System.Text.Json;
using ECommerce.Application.Abstract.Service;
using ECommerce.Shared.Constants;
using RabbitMQ.Client;

namespace ECommerce.Application.Services.Queue;

public class QueueService : IMessageBroker, IDisposable
{
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly ILoggingService _logger;

    public QueueService(IConnectionFactory connectionFactory, ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        try
        {
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.QueueConnectionFailed, ex.Message);
            _connection = null;
            _channel = null;
            return;
        }
    }

    public async Task PublishAsync<T>(T message, string exchange, string routingKey) where T : class
    {
        try
        {
            await Task.Run(() =>
            {
                _channel.ExchangeDeclare(exchange, ExchangeType.Direct, true);
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                _channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: properties, body: body);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.QueueMessagePublishFailed, ex.Message);
            return;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
using System.Text;
using System.Text.Json;
using ECommerce.Application.Abstract.Service;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace ECommerce.Application.Services.Queue;

public class RabbitMQService : IMessageBroker, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILoggingService _logger;

    public RabbitMQService(IConfiguration configuration, ILoggingService logger)
    {
        _logger = logger;
        
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"],
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672")
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _logger.LogInformation("Connected to RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
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

                _channel.BasicPublish(
                    exchange: exchange,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);
            });

            _logger.LogInformation($"Message published to exchange: {exchange}, routing key: {routingKey}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to RabbitMQ");
            throw;
        }
    }


    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
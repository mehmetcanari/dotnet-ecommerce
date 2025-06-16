using System.Text;
using System.Text.Json;
using ECommerce.Application.Abstract.Service;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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

    public async Task PublishAsync<T>(T message, string exchange, string routingKey)
    {
        try
        {
            await Task.Run(() =>
            {
                _channel.ExchangeDeclare(exchange, ExchangeType.Direct, true);
                
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true; // Mesajların kalıcı olmasını sağlar

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

    public async Task SubscribeAsync<T>(string queue, Func<T, Task> handler)
    {
        await Task.Run(() =>
        {
            _channel.QueueDeclare(queue, true, false, false, null);
            
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));
                    
                    if (message != null)
                    {
                        await handler(message);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Received null message from queue: {Queue}", queue);
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue: {Queue}", queue);
                    _channel.BasicNack(ea.DeliveryTag, false, true); // Mesajı tekrar kuyruğa al
                }
            };

            _channel.BasicConsume(queue, false, consumer);
        });
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
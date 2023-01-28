﻿using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WhiteRabbit.Messaging.Abstractions;

namespace WhiteRabbit.Messaging.RabbitMq;

internal class MessageManager : IMessageSender, IDisposable
{
    private const string MaxPriorityHeader = "x-max-priority";

    internal IConnection Connection { get; private set; }

    internal IModel Channel { get; private set; }

    private readonly MessageManagerSettings messageManagerSettings;
    private readonly QueueSettings queueSettings;

    public MessageManager(MessageManagerSettings messageManagerSettings, QueueSettings queueSettings)
    {
        var factory = new ConnectionFactory { Uri = new Uri(messageManagerSettings.ConnectionString) };
        Connection = factory.CreateConnection();

        Channel = Connection.CreateModel();

        if (messageManagerSettings.QueuePrefetchCount > 0)
        {
            Channel.BasicQos(0, messageManagerSettings.QueuePrefetchCount, false);
        }
        
        Channel.ExchangeDeclare(messageManagerSettings.ExchangeName,
            type: ExchangeType.Direct,
            true,
            false);

        Channel.ExchangeDeclare($"{messageManagerSettings.ExchangeName}_retry",
            type: ExchangeType.Direct,
            true,
            false);

        foreach (var queue in queueSettings.Queues)
        {
            var args = new Dictionary<string, object>
            {
                [MaxPriorityHeader] = 10,
                ["x-dead-letter-exchange"] = $"{messageManagerSettings.ExchangeName}_retry",
            };

            Channel.QueueDeclare(queue.Name, durable: true, exclusive: false, autoDelete: true, args);
            Channel.QueueBind(queue.Name, messageManagerSettings.ExchangeName, $"{queue.Name}");

            //------------------

            var args2 = new Dictionary<string, object>
            {
                [MaxPriorityHeader] = 10,
                ["x-dead-letter-exchange"] = $"{messageManagerSettings.ExchangeName}",
                ["x-message-ttl"] = 5000
            };
            Channel.QueueDeclare($"{queue.Name}_retry", durable: true, exclusive: false, autoDelete: true, arguments: args2);
            Channel.QueueBind($"{queue.Name}_retry", $"{messageManagerSettings.ExchangeName}_retry", $"{queue.Name}"); 

        }

        this.messageManagerSettings = messageManagerSettings;
        this.queueSettings = queueSettings;
    }

    public Task PublishAsync<T>(T message, int priority = 1) where T : class
    {
        var sendBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<object>(message, messageManagerSettings.JsonSerializerOptions ?? JsonOptions.Default));

        var routingKey = queueSettings.Queues.First(q => q.Type == typeof(T)).Name;
        return PublishAsync(sendBytes.AsMemory(), routingKey, priority);
    }

    private Task PublishAsync(ReadOnlyMemory<byte> body, string routingKey, int priority = 1)
    {
        var properties = Channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.Priority = Convert.ToByte(priority);

        Channel.BasicPublish(messageManagerSettings.ExchangeName, routingKey, properties, body);
        return Task.CompletedTask;
    }

    public void MarkAsComplete(BasicDeliverEventArgs message) => Channel.BasicAck(message.DeliveryTag, false);

    public void MarkAsRejected(BasicDeliverEventArgs message) => Channel.BasicNack(message.DeliveryTag, false, false);
    

    public void Dispose()
    {
        try
        {
            if (Channel.IsOpen)
            {
                Channel.Close();
            }

            if (Connection.IsOpen)
            {
                Connection.Close();
            }
        }
        catch
        {
        }

        GC.SuppressFinalize(this);
    }
}

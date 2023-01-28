﻿using WhiteRabbit.Messaging.Abstractions;
using WhiteRabbit.Shared;

namespace WhiteRabbit.Receivers;

public class OrderMessageReceiver : IMessageReceiver<Order>
{
    private readonly ILogger logger;
    private readonly IMessageSender messageSender;

    public OrderMessageReceiver(ILogger<OrderMessageReceiver> logger, IMessageSender messageSender)
    {
        this.logger = logger;
        this.messageSender = messageSender;
    }

    public async Task ReceiveAsync(Order message, CancellationToken cancellationToken)
    {
        logger.LogInformation("ORDER : Processing order {OrderNumber}...", message.Number);

        await Task.Delay(TimeSpan.FromMilliseconds(100 + Random.Shared.Next(10)));

        logger.LogInformation("ORDER : End processing order {OrderNumber}", message.Number);

        await messageSender.PublishAsync(new Invoice { OrderNumber = message.Number });
    }
}

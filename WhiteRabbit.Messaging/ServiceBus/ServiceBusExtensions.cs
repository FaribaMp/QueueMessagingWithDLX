﻿using Microsoft.Extensions.DependencyInjection;
using System;
using WhiteRabbit.Messaging.Abstractions;

namespace WhiteRabbit.Messaging.ServiceBus
{
    public static class ServiceBusExtensions
    {
        public static IMessagingBuilder AddServiceBus(this IServiceCollection services, Action<MessageManagerSettings> messageManagerConfiguration = null, Action<QueueSettings> queuesConfiguration = null)
        {
            services.AddSingleton<MessageManager>();
            services.AddSingleton<IMessageSender>(provider => provider.GetService<MessageManager>());

            if (messageManagerConfiguration != null)
            {
                var messageManagerSettings = new MessageManagerSettings();
                messageManagerConfiguration.Invoke(messageManagerSettings);
                services.AddSingleton(messageManagerSettings);
            }

            if (queuesConfiguration != null)
            {
                var queueSettings = new QueueSettings();
                queuesConfiguration.Invoke(queueSettings);
                services.AddSingleton(queueSettings);
            }

            return new DefaultMessagingBuilder(services);
        }

        public static IMessagingBuilder AddReceiver<TObject, TReceiver>(this IMessagingBuilder builder) where TObject : class
            where TReceiver : class, IMessageReceiver<TObject>
        {
            builder.Services.AddHostedService<QueueListener<TObject>>();
            builder.Services.AddTransient<IMessageReceiver<TObject>, TReceiver>();

            return builder;
        }
    }
}

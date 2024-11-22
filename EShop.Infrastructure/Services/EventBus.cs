using EShop.Application.Abstractions.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Infrastructure.Services;

internal sealed class EventBus(IPublishEndpoint publishEndpoint, ILogger<EventBus> logger)
        : IEventBus
{
    public Task PublishAsync<TMessage>(TMessage message)
        where TMessage : class
    {
        logger.LogInformation("Publishing event: {event}", typeof(TMessage).Name);
        publishEndpoint.Publish(message);
        logger.LogInformation("Event {event} have been published successfully", typeof(TMessage).Name);
        return Task.CompletedTask;
    }
}
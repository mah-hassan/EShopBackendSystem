namespace EShop.Application.Abstractions.Services;

public interface IEventBus
{

    Task PublishAsync<TMessage>(TMessage message)
        where TMessage : class;
}
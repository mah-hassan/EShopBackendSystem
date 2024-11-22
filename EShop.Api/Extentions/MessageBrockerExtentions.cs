using EShop.Application.Abstractions.Services;
using EShop.Domain.Settings;
using MassTransit;
using Microsoft.Extensions.Options;

namespace EShop.Api.Extentions
{
    public static class MessageBrockerExtentions
    {
        public static IServiceCollection AddMessageBrocker(this IServiceCollection services)
        {
            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

                busConfigurator.AddConsumers(typeof(IEventBus).Assembly);

                busConfigurator.UsingRabbitMq((context, rmqConfigurator) =>
                {
                    var settings = context.GetRequiredService<IOptionsMonitor<MessageBrockerSettings>>().CurrentValue;
                    rmqConfigurator.Host(new Uri(settings.Host), h =>
                    {
                        h.Username(settings.UserName);
                        h.Password(settings.Password);

                    });
                    rmqConfigurator.UseMessageRetry(r =>
                    {
                        r.Exponential(3,
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(2),
                            TimeSpan.FromSeconds(3)); 
                    });
                    rmqConfigurator.ConfigureEndpoints(context);
                });
            });
            return services;
        }
    }
}
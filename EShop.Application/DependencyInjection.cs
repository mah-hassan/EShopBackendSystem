using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Supabase;
using MediatR;
using EShop.Application.Abstractions.Mappers;
using EShop.Application.Behaviors;

namespace EShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
  
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingPipelineBehavior<,>));
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            config.AddOpenBehavior(typeof(CachingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
        });

        services.AddScoped<Mapper>();
        return services;
    }
}

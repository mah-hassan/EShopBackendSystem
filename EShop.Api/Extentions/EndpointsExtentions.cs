using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EShop.Api.Extentions;

public static class EndpointsExtentions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        IEnumerable<ServiceDescriptor> endpointsServiceDescriptors = typeof(EndpointsExtentions).Assembly
           .DefinedTypes
           .Where(t => t is { IsAbstract: false, IsInterface: false} &&
           t.IsAssignableTo(typeof(IEndpoint)))
           .Select(endpoint => ServiceDescriptor.Transient(typeof(IEndpoint), endpoint));

        services.TryAddEnumerable(endpointsServiceDescriptors);
        return services;
    }
    
    public static IApplicationBuilder MapEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        foreach (var endpoint in endpoints)
        {
            endpoint.AddRoutes(app);
        }

        return app;
    }
}

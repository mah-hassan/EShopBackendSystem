using MediatR;
using Microsoft.Extensions.Logging;

namespace EShop.Application.Behaviors;

public sealed class CachingPipelineBehavior<TQuery, TResponse>(
    ILogger<CachingPipelineBehavior<TQuery, TResponse>> logger,
    ICachService cachService)
    : IPipelineBehavior<TQuery, TResponse>
    where TQuery : ICachedQuery
    where TResponse : class
{
    public async Task<TResponse> Handle(TQuery request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Caching {typeof(TQuery).Name} with key: {request.CachKey}");
        TResponse? cachedResult = null;

        try
        {
            cachedResult = await cachService.GetAsync<TResponse>(request.CachKey);
        }
        catch(Exception ex) 
        {
            logger.LogError(ex, "An error occurred while retrieving cache for {queryType} with key: {key}",
                typeof(TQuery), request.CachKey);
        }
        if (cachedResult is not null)
            return cachedResult;

        var result = await next();

        try
        {
            await cachService.AddOrUpdateAsync(result, request.CachKey, request.Period);
        }
        catch (Exception ex)
        {

            logger.LogError(ex, "An error occurred while adding or updating cache for {queryType} with key: {key}",
                            typeof(TQuery), request.CachKey);
        }
        return result;
    }
}
using MediatR;
using Microsoft.Extensions.Logging;


namespace EShop.Application.Behaviors;

public class LoggingPipelineBehavior<TQuery, TResponse>
    : IPipelineBehavior<TQuery, TResponse>
    where TQuery : IRequest<TResponse>
    where TResponse : Result

{
    private readonly ILogger<LoggingPipelineBehavior<TQuery, TResponse>> _logger;

    public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TQuery, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TQuery request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {request}", typeof(TQuery).Name);
        var response = await next();
        _logger.LogInformation("Handled {request} with response: {res}", typeof(TQuery).Name, response.ToString());
        return response;
    }
}
using MediatR;

namespace EShop.Application.Abstractions.Messaging;

public interface IQuery<TResponse>
    : IRequest<Result<TResponse>>
    where TResponse : class;

public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
    where TResponse : class;

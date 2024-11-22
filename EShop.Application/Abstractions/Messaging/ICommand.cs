using MediatR;
using EShop.Domain.Shared;

namespace EShop.Application.Abstractions.Messaging;

public interface ICommand
    : IRequest<Result>;
   
public interface ICommand<TRespose>
    : IRequest<Result<TRespose>>;

public interface ICommandHandler<TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
namespace EShop.Application.Abstractions.Messaging;


public interface ICachedQuery<TResponse> : IQuery<TResponse>, ICachedQuery
    where TResponse : class;
public interface ICachedQuery
{
    string CachKey { get;  }
    TimeSpan? Period { get; }
}
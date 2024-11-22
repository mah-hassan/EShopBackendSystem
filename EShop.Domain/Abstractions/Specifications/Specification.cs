using System.Linq.Expressions;

namespace EShop.Domain.Abstractions.Specifications;

public abstract class Specification<TEntity>
    where TEntity : Entity
{
    public int? Take { get; protected set; }
    public int? PageNumber { get; protected set; }
    public Expression<Func<TEntity, object>>? OrderByAscExpression { get; protected set; }
    public Expression<Func<TEntity, object>>? OrderByDescExpression { get; protected set; }
    public List<Expression<Func<TEntity, bool>>> Criterias { get; private set; } = new();
    public List<Expression<Func<TEntity, object>>> IncludeExpressions { get; private set; } = new();

    protected void AddInclude(Expression<Func<TEntity, object>> expression)
    {
        IncludeExpressions.Add(expression);
    }

    protected void AddCriteria(Expression<Func<TEntity, bool>> criteria)
    {
        Criterias.Add(criteria);
    }
}
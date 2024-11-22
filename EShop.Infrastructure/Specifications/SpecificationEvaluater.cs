using EShop.Domain.Abstractions;
using EShop.Domain.Abstractions.Specifications;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.Specifications;

internal static class SpecificationEvaluater
 
{
    public static IQueryable<TEntity> Evaluate<TEntity>(this IQueryable<TEntity> inputQuery,
        Specification<TEntity> specification)
        where TEntity : Entity
    {
        var querable = inputQuery;

        if(specification.IncludeExpressions.Any())
        {
            querable = specification.IncludeExpressions
                .Aggregate(querable,
                    (current, include) => current.Include(include));
        }

        if(specification.Criterias.Any())
        {
            querable = specification.Criterias
                .Aggregate(querable,
                    (current, criteria) => current.Where(criteria));
        }

        if (specification.OrderByAscExpression is not null)
        {
            querable = querable.OrderBy(specification.OrderByAscExpression);
        }

        else if (specification.OrderByDescExpression is not null)
        {
            querable = querable.OrderBy(specification.OrderByDescExpression);
        }

        if (specification.PageNumber.HasValue && specification.Take.HasValue)
        {
            querable = querable
                .Take(specification.Take.Value)
                .Skip((specification.PageNumber.Value - 1) * specification.Take.Value);
        }

        return querable;
    }
}
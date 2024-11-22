using EShop.Domain.Abstractions.Specifications;
using EShop.Domain.Products;

namespace EShop.Application.Products.Queries.GetProducts;

public sealed class GetProductsSecification
    : Specification<Product>
{
    public GetProductsSecification(GetProductsQuery query)
    {
        if (query.brandId is not null)
        {
            AddCriteria(p => p.BrandId == query.brandId);
        }
        if (query.categoryId is not null)
        {
            AddCriteria(p => p.CategoryId == query.categoryId);
        }
        if (!string.IsNullOrWhiteSpace(query.orderBy))
        {
            string direction = query.orderType ?? "ASC";
            AddOrderBY(query.orderBy, direction);
        }
        if (query.pageNumber.HasValue && query.size.HasValue)
        {
            PageNumber = query.pageNumber.Value;
            Take = query.size.Value;
        }
    }

    private void AddOrderBY(string orderBy, string direction)
    {

        if (direction.Equals("ASC", StringComparison.OrdinalIgnoreCase))
        {
            OrderByAscExpression = orderBy.ToLower() switch
            {
                "name" => p => p.Name,
                "rate" => p => p.Reviews.Count,
                _ => p => p.Name
            };
        }
        else
        {
            OrderByDescExpression = orderBy.ToLower() switch
            {
                "name" => p => p.Name,
                "rate" => p => p.Reviews.Count,
                _ => p => p.Name
            };
        }
    }

    public GetProductsSecification DisablePagenation()
    {
        Take = null;
        PageNumber = null;
        return this;
    }
}
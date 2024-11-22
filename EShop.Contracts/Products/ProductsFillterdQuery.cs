namespace EShop.Contracts.Products;


public sealed record ProductsFillterdQuery(
    Guid? categoryId,
    Guid? brandId,
    string? orderBy,
    string? orderType,
    int? pageNumber,
    int? size);
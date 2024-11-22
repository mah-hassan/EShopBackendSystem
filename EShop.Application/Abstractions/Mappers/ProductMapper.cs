using EShop.Application.Products.Queries.Search;
using EShop.Contracts.Products;
using EShop.Contracts.Reviews;
using EShop.Contracts.ShoppingCart;
using EShop.Domain.Products;
using EShop.Domain.ShoppingCarts;
using Riok.Mapperly.Abstractions;

namespace EShop.Application.Abstractions.Mappers;

public partial class Mapper
{
    [MapperIgnoreTarget(nameof(Product.PrimaryImage))]
    [MapperIgnoreTarget(nameof(Product.Images))]
    public partial Product MapToProduct(ProductRequest source);



    [MapperIgnoreTarget(nameof(ProductDetails.Images))]
    [MapperIgnoreTarget(nameof(ProductDetails.PrimaryImage))]
    [MapperIgnoreTarget(nameof(ProductDetails.Variants))]
    public partial ProductDetails MapToProductDetails(Product product);



    [MapperIgnoreTarget(nameof(ProductResponse.PrimaryImage))]
    public partial ProductResponse MapToProductResponse(Product product);


    public partial ProductResponse MapToProductResponse(ElasticSearchProduct product);


    public partial ProductLineItem MapToProductLineItem(ShoppingCartItem item);


    public partial Review MapToReview(ReviewRequest review);

    public partial ReviewResponse MapToReviewResponse(Review review);
}
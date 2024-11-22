using EShop.Application.Products.Queries.GetProducts;
using EShop.Domain.Brands;
using EShop.Domain.Products;
using EShop.Test.SharedUtilities.Products;
using FluentAssertions;
using System.Linq;

namespace EShop.Test.Application.Products.Queries.GetProducts;

public sealed class GetProductsSpecificationTests
{
    [Fact]
    public void Constructor_AddsBrandCriteria_WhenBrandIdProvided()
    {
        // Arrange
        var brandId = Guid.NewGuid();
        var query = new GetProductsQuery(null, brandId, null, null, null, null);

        // Act
        var specification = new GetProductsSecification(query);

        // Assert
        specification.Criterias.Should().ContainSingle()
               .Which.Compile()(ProductFaker.CreateTestProduct(brandId: brandId)).Should().BeTrue();
    }


    [Fact]
    public void Constructor_AddsCategoryCriteria_WhenCategoryIdProvided()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetProductsQuery(categoryId, null, null, null, null, null);

        // Act
        var specification = new GetProductsSecification(query);

        // Assert

        specification.Criterias.Should().ContainSingle()
           .Which.Compile()(ProductFaker.CreateTestProduct(categoryId: categoryId)).Should().BeTrue();

    }

    [Fact]
    public void Constructor_AddsOrderByNameAsc_WhenOrderByIsNameAndOrderTypeIsAsc()
    {
        // Arrange
        var query = new GetProductsQuery(null, null, "name", "ASC", null, null);
        var specification = new GetProductsSecification(query);
        var products = new List<Product>
        {
            ProductFaker.CreateTestProduct(name: "Apple"),
            ProductFaker.CreateTestProduct(name: "Banana"),
            ProductFaker.CreateTestProduct(name: "Cherry")
        };

        // Act
        var orderedProducts = products.OrderBy(specification.OrderByAscExpression!.Compile()).ToList();

        // Assert
        orderedProducts.Select(p => p.Name).Should().Equal("Apple", "Banana", "Cherry");
    }

    [Fact]
    public void Constructor_AddsOrderByRateDesc_WhenOrderByIsRateAndOrderTypeIsDesc()
    {
        // Arrange
        var query = new GetProductsQuery(null, null, "rate", "DESC", null, null);
        var specification = new GetProductsSecification(query);
        var products = new List<Product>
        {
            ProductFaker.CreateTestProduct().WithReviews(5) , 
            ProductFaker.CreateTestProduct().WithReviews(10),
            ProductFaker.CreateTestProduct().WithReviews(8)
        };

        // Act
        var orderedProducts = products.OrderByDescending(specification.OrderByDescExpression!.Compile()).ToList();

        // Assert
        orderedProducts.Select(p => p.Reviews.Count).Should().Equal(10, 8, 5);
    }

    [Fact]
    public void Constructor_SetsPagingValues_WhenPageNumberAndSizeProvided()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 10;
        var query = new GetProductsQuery(null, null, null, null, pageNumber, pageSize);

        // Act
        var specification = new GetProductsSecification(query);

        // Assert
        specification.PageNumber.Should().Be(pageNumber);
        specification.Take.Should().Be(pageSize);
    }

    [Fact]
    public void Constructor_DefaultsToNameOrderAsc_WhenOrderByIsInvalid()
    {
        // Arrange
        var query = new GetProductsQuery(null, null, "invalid_order", "ASC", null, null);
        var specification = new GetProductsSecification(query);
        var products = new List<Product>
        {
            ProductFaker.CreateTestProduct(name: "Cherry"),
            ProductFaker.CreateTestProduct(name: "Banana"),
            ProductFaker.CreateTestProduct(name: "Apple")
        };

        // Act
        var orderedProducts = products.OrderBy(specification.OrderByAscExpression!.Compile()).ToList();

        // Assert
        orderedProducts.Select(p => p.Name).Should().Equal("Apple", "Banana", "Cherry");
    }
}
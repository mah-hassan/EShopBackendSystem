using EShop.Application.Abstractions.Mappers;
using EShop.Application.Abstractions.Services;
using EShop.Application.Products.Queries.GetProducts;
using EShop.Domain.Products;
using EShop.Test.SharedUtilities.Products;
using FluentAssertions;
using Moq;

namespace EShop.Test.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandlerTests
{
    private readonly List<Product> _products;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly Mock<ISupabaseService> _supabaseServiceMock;
    private readonly Mapper _mapper;

    public GetProductsQueryHandlerTests()
    {
        _products = ProductFaker.GetListOfProducts();
        _productRepositoryMock = new();
        _reviewRepositoryMock = new();
        _supabaseServiceMock = new();
        _mapper = new();
    }

    private GetProductsQueryHandler CreateHandler()
    {
        return new(_productRepositoryMock.Object, _reviewRepositoryMock.Object, _mapper, _supabaseServiceMock.Object);
    }

    [Fact]
    public async Task Handle_FiltersByCategory_WhenCategoryIdProvided()
    {
        // Arrange
        var categoryId = _products[new Random().Next(_products.Count)].CategoryId;
        var query = new GetProductsQuery(categoryId, null, null, null, 1, 10);

        _productRepositoryMock.Setup(repo => repo.GetProductsWithSpecificationAsync(It.IsAny<GetProductsSecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_products.FindAll(p => p.CategoryId == categoryId));
        _productRepositoryMock.Setup(repo => repo.CountAsync(It.IsAny<GetProductsSecification>()))
            .ReturnsAsync(_products.Count);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value?.Items.Should().OnlyContain(item => item.CategoryId == categoryId);
    }

    [Fact]
    public async Task Handle_FiltersByBrand_WhenBrandIdProvided()
    {
        // Arrange
        var brandId = _products[new Random().Next(_products.Count)].BrandId;
        var query = new GetProductsQuery(null, brandId, null, null, 1, 10);

        _productRepositoryMock.Setup(repo => repo.GetProductsWithSpecificationAsync(It.IsAny<GetProductsSecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_products.FindAll(p => p.BrandId == brandId));
        _productRepositoryMock.Setup(repo => repo.CountAsync(It.IsAny<GetProductsSecification>()))
            .ReturnsAsync(_products.Count);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value?.Items.Should().OnlyContain(item => item.BrandId == brandId);
    }

}
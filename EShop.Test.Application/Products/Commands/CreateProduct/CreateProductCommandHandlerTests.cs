using EShop.Application.Abstractions;
using EShop.Application.Abstractions.Mappers;
using EShop.Application.Abstractions.Services;
using EShop.Application.Products.Commands.CreateProduct;
using EShop.Domain.Brands;
using EShop.Domain.Categories;
using EShop.Domain.Entities;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using EShop.Test.SharedUtilities.Products;

namespace EShop.Test.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISupabaseService> _supabaseServiceMock;
    private readonly Mock<IProductAttribuatesRepository> _productAttribuatesRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICategoryBrandsRepository> _categoryBrandsRepositoryMock;
    private readonly Mock<IBrandRepository> _brandRepositoryMock;
    private readonly Mapper _mapper;

    public CreateProductCommandHandlerTests()
    {
        _productRepositoryMock = new();
        _unitOfWorkMock = new();
        _supabaseServiceMock = new();
        _productAttribuatesRepositoryMock = new();
        _mapper = new();
        _eventBusMock = new();
        _categoryRepositoryMock = new();
        _categoryBrandsRepositoryMock = new();
        _brandRepositoryMock = new();
    }

    private CreateProductCommandHandler CreateHandler()
    {
        return new CreateProductCommandHandler(
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _supabaseServiceMock.Object,
            _productAttribuatesRepositoryMock.Object,
            _brandRepositoryMock.Object,
            _eventBusMock.Object,
            _categoryRepositoryMock.Object,
            _categoryBrandsRepositoryMock.Object,
            _mapper);
    }


    [Fact]
    public async Task Handle_ShouldReturnProductId_WhenProductIsCreatedSuccessfully()
    {
        // Arrange
        var productRequest = ProductRequestFaker.CreateProductRequest();
        var request = new CreateProductCommand(productRequest);
        var handler = CreateHandler();

        // Mock SKU uniqueness check
        _productRepositoryMock
            .Setup(x => x.IsSkuUnique(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Mock brand and category exist
        _brandRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Brand { Id = request.ProductRequest.BrandId, Name = "x brand" });
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Category { Id = request.ProductRequest.CategoryId, Name = "x category" });

        // Mock successful image upload to Supabase
        _supabaseServiceMock
            .Setup(x => x.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("uploaded-primary-image-url");

        // Mock attribute addition
        _productAttribuatesRepositoryMock
            .Setup(x => x.AddProductAttribuate(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Verifiable();

        // Mock product repository and UnitOfWork save
        _productRepositoryMock.Setup(x => x.Add(It.IsAny<Product>())).Verifiable();
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Mock event bus
        _eventBusMock
            .Setup(x => x.PublishAsync(It.IsAny<ProductCreatedEvent>()))
            .Verifiable();

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _productRepositoryMock.Verify(x => x.Add(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ProductCreatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFailWithConflictError_WhenSkuIsNotUnique()
    {
        // Arrange
        var request = new CreateProductCommand(ProductRequest: ProductRequestFaker.CreateProductRequest());
        var handler = CreateHandler();

        // Mock SKU uniqueness check to return false (SKU not unique)
        _productRepositoryMock
            .Setup(x => x.IsSkuUnique(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Single().Code.Should().Be("Sku");
        result.Errors.Single().Type.Should().Be(ErrorType.Conflict);
        result.Errors.Single().Message.Should().Be("Product Sku already exists");
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFoundError_WhenBrandNotFound()
    {
        // Arrange
        var request = new CreateProductCommand(ProductRequest: ProductRequestFaker.CreateProductRequest());
        var handler = CreateHandler();

        // Mock SKU uniqueness check to return true (SKU is unique)
        _productRepositoryMock
            .Setup(x => x.IsSkuUnique(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Mock brand to return null (brand not found)
        _brandRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(default(Brand));

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Single().Code.Should().Be("Brand");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
        result.Errors.Single().Message.Should().Be("Brand not found");
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFoundError_WhenCategoryNotFound()
    {
        // Arrange
        var request = new CreateProductCommand(ProductRequest: ProductRequestFaker.CreateProductRequest());
        var handler = CreateHandler();

        // Mock SKU uniqueness check to return true (SKU is unique)
        _productRepositoryMock
            .Setup(x => x.IsSkuUnique(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Mock brand to return valid object
        _brandRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Brand { Id = request.ProductRequest.BrandId, Name = "x brand" });

        // Mock category to return null (category not found)
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(default(Category));

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Single().Code.Should().Be("Category");
        result.Errors.Single().Message.Should().Be("Category not found");
    }
}
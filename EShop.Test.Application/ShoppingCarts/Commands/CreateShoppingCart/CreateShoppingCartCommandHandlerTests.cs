using EShop.Application.Abstractions.Mappers;
using EShop.Application.Abstractions.Services;
using EShop.Application.ShoppingCarts.Commands.CreateCart;
using EShop.Contracts.ShoppingCart;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ShoppingCarts;
using EShop.Test.SharedUtilities.Common;
using EShop.Test.SharedUtilities.Products;
using EShop.Test.SharedUtilities.ShoppingCarts;
using EShop.Test.SharedUtilities;
using Microsoft.AspNetCore.Http;
using Moq;
using EShop.Application.Common.Constants;
using FluentAssertions;

namespace EShop.Test.Application.ShoppingCarts.Commands.CreateShoppingCart;

public sealed class CreateShoppingCartCommandHandlerTests
{
    private readonly Mock<IShoppingCartRepository> _shoppingCartRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<ISupabaseService> _supabaseServiceMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mapper _mapper = new(); 
    private readonly CreateShoppingCartCommandHandler _handler;

    public CreateShoppingCartCommandHandlerTests()
    {
        _handler = new CreateShoppingCartCommandHandler(
            _shoppingCartRepositoryMock.Object,
            _supabaseServiceMock.Object,
            _httpContextAccessorMock.Object,
            _productRepositoryMock.Object,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldFailWithConflict_WhenUserAlreadyHasShoppingCart()
    {
        // Arrange
        var existingCart = ShoppingCartFaker.Create();
        var command = new CreateShoppingCartCommand(new HashSet<ProductLineItemRequest> { ProductLineItemRequestFaker.Create() });
        var userId = existingCart.UserId;

        _httpContextAccessorMock.Setup(ctx => ctx.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(userId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(existingCart);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("this user has an exsisting Shopping Cart");
        result.Errors.Single().Code.Should().Be("Shopping Cart");
        result.Errors.Single().Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var command = new CreateShoppingCartCommand(new HashSet<ProductLineItemRequest> { ProductLineItemRequestFaker.Create() });
        var userId = Guid.NewGuid();

        _httpContextAccessorMock.Setup(ctx => ctx.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(userId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync((ShoppingCart?)null);
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Product not found");
        result.Errors.Single().Code.Should().Be("Product");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldFailWithBadRequest_WhenVariantDoesNotExistInProduct()
    {
        // Arrange
        var command = new CreateShoppingCartCommand(new HashSet<ProductLineItemRequest> { ProductLineItemRequestFaker.Create(includeVariants: false) });
        var userId = Guid.NewGuid();
        var product = ProductFaker.CreateTestProduct(includeVariants: false);

        command.Items.First().Variants.Add("NonExistingVariant", "SomeValue");

        _httpContextAccessorMock.Setup(ctx => ctx.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(userId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync((ShoppingCart?)null);
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Variant 'NonExistingVariant' not found for this product");
        result.Errors.Single().Code.Should().Be("Product.Variant");
        result.Errors.Single().Type.Should().Be(ErrorType.BadRequest);
    }

    [Fact]
    public async Task Handle_ShouldFailWithBadRequest_WhenVariantOptionIsNotAvailableForProduct()
    {
        // Arrange
        var command = new CreateShoppingCartCommand(new HashSet<ProductLineItemRequest> { ProductLineItemRequestFaker.Create(includeVariants: false) });
        var userId = Guid.NewGuid();
        var product = ProductFaker.CreateTestProduct();

        command.Items.First().Variants.Add("Color", "NonAvailableColor");

        _httpContextAccessorMock.Setup(ctx => ctx.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(userId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync((ShoppingCart?)null);
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Option 'NonAvailableColor' not available in this product`s Colors");
        result.Errors.Single().Code.Should().Be("Product.Variant.Option");
        result.Errors.Single().Type.Should().Be(ErrorType.BadRequest);
    }

    [Fact]
    public async Task Handle_ShouldCreateShoppingCartSuccessfully_WhenInputsIsValidAndNoCartEsxixts()
    {
        // Arrange
        var command = new CreateShoppingCartCommand(new HashSet<ProductLineItemRequest> { ProductLineItemRequestFaker.Create(includeVariants: false) });
        var userId = Guid.NewGuid();
        var product = ProductFaker.CreateTestProduct();
        var productColors = product.Variants.Single(v => v.Key.Name.Equals("Color", StringComparison.OrdinalIgnoreCase));

        command.Items.First().Variants.Add("Color", productColors.First().Value);
        _httpContextAccessorMock.Setup(ctx => ctx.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(userId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync((ShoppingCart?)null);
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);
        _supabaseServiceMock.Setup(s => s.GetPublicUrl(SupabaseBackets.Products, product.PrimaryImage)).Returns("https://example.com/image.png");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.Should().NotBeNull();
        response?.Items.Should().HaveCount(command.Items.Count);
    }
}

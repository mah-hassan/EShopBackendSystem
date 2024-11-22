using EShop.Application.Abstractions.Mappers;
using EShop.Application.Abstractions.Services;
using EShop.Application.Common.Constants;
using EShop.Application.ShoppingCarts.Commands.AddItemsToCart;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ShoppingCarts;
using EShop.Test.SharedUtilities;
using EShop.Test.SharedUtilities.Common;
using EShop.Test.SharedUtilities.Products;
using EShop.Test.SharedUtilities.ShoppingCarts;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace EShop.Test.Application.ShoppingCarts.Commands.AddItemToCart;

public sealed class AddItemToCartCommandHandlerTests
{

    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IShoppingCartRepository> _shoppingCartRepositoryMock = new();
    private readonly Mock<ISupabaseService> _supabaseServiceMock = new();
    private readonly Mapper _mapper = new();
    private readonly Mock<IHttpContextAccessor> _contextAccessorMock = new();
    private readonly AddItemToCartCommandHandler _handler;

    public AddItemToCartCommandHandlerTests()
    {
        _handler = new AddItemToCartCommandHandler(
            _productRepositoryMock.Object,
            _shoppingCartRepositoryMock.Object,
            _supabaseServiceMock.Object,
            _mapper,
            _contextAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenCartDoesNotExist()
    {
        // Arrange
        var command = new AddItemToCartCommand(ProductLineItemRequestFaker.Create());
        _contextAccessorMock.Setup(ctx => ctx.HttpContext).Returns(HttpContextMockProvider.GetHttpContext());
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((ShoppingCart?)null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("user has not created a Shopping cart ");
        result.Errors.Single().Code.Should().Be("ShoppingCart");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldFailWithConflict_WhenItemAlreadyExistsInCart()
    {
        // Arrange
        var cart = ShoppingCartFaker.Create();
        var command = new AddItemToCartCommand(ProductLineItemRequestFaker.Create());
        cart.Items.Add(new ShoppingCartItem { ProductId = command.Item.ProductId });

        _contextAccessorMock.Setup(ctx => ctx.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(cart.UserId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(cart.UserId))
            .ReturnsAsync(cart);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Item already exists in cart");
        result.Errors.Single().Code.Should().Be("ShoppingCartItem");
        result.Errors.Single().Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var cart = ShoppingCartFaker.Create();
        var command = new AddItemToCartCommand(ProductLineItemRequestFaker.Create());

        _contextAccessorMock.Setup(ctx => ctx.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(cart.UserId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(cart.UserId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Item.ProductId)).ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Product not found");
        result.Errors.Single().Code.Should().Be("Product");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldAddItemToCart_WhenProductAndCartExist()
    {
        // Arrange
        var cart = ShoppingCartFaker.Create();
        var command = new AddItemToCartCommand(ProductLineItemRequestFaker.Create(includeVariants: false));
        var product = ProductFaker.CreateTestProduct();
        _contextAccessorMock.Setup(ctx => ctx.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(cart.UserId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(cart.UserId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Item.ProductId)).ReturnsAsync(product);
        _supabaseServiceMock.Setup(s => s.GetPublicUrl(SupabaseBackets.Products, product.PrimaryImage)).Returns("https://test.url/test.png");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _shoppingCartRepositoryMock.Verify(repo => repo.CreateOrUpdateAsync(cart), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFailWithBadRequest_WhenVariantDoesNotExistInProduct()
    {
        // Arrange
        var cart = ShoppingCartFaker.Create();
        var command = new AddItemToCartCommand(ProductLineItemRequestFaker.Create(includeVariants: false));
        var product = ProductFaker.CreateTestProduct(includeVariants: false);
        
        command.Item.Variants.Add("NonExistingVariant", "SomeValue");

        _contextAccessorMock.Setup(ctx => ctx.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(cart.UserId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(cart.UserId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Item.ProductId)).ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be($"Variant 'NonExistingVariant' not found for this product");
        result.Errors.Single().Code.Should().Be("Product.Variant");
        result.Errors.Single().Type.Should().Be(ErrorType.BadRequest);
    }

    [Fact]
    public async Task Handle_ShouldFailWithBadRequest_WhenVariantOptionIsNotAvailableForProduct()
    {
        // Arrange
        var cart = ShoppingCartFaker.Create();
        var command = new AddItemToCartCommand(ProductLineItemRequestFaker.Create(includeVariants: false));
        var product = ProductFaker.CreateTestProduct();

        command.Item.Variants.Add("Color", "NonAvailableColor");

        _contextAccessorMock.Setup(ctx => ctx.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(cart.UserId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(cart.UserId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Item.ProductId)).ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Option 'NonAvailableColor' not available in this product`s Colors");
        result.Errors.Single().Code.Should().Be("Product.Variant.Option");
        result.Errors.Single().Type.Should().Be(ErrorType.BadRequest);
    }
}
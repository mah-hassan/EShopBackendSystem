using EShop.Application.Abstractions.Mappers;
using EShop.Application.Abstractions.Services;
using EShop.Application.ShoppingCarts.Commands.UpdateShoppingCartItem;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ShoppingCarts;
using EShop.Test.SharedUtilities.Common;
using EShop.Test.SharedUtilities.Products;
using EShop.Test.SharedUtilities.ShoppingCarts;
using EShop.Test.SharedUtilities;
using Microsoft.AspNetCore.Http;
using Moq;
using FluentAssertions;
using EShop.Application.Common.Constants;

namespace EShop.Test.Application.ShoppingCarts.Commands.UpdateShoppingCartItem;

public sealed class UpdateShoppingCartItemCommandHandlerTests
{
    private readonly Mock<IShoppingCartRepository> _shoppingCartRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IHttpContextAccessor> _contextAccessorMock = new();
    private readonly Mock<ISupabaseService> _supabaseServiceMock = new();
    private readonly Mapper _mapper = new(); 
    private readonly UpdateShoppingCartItemCommandHandler _handler;

    public UpdateShoppingCartItemCommandHandlerTests()
    {
        _handler = new UpdateShoppingCartItemCommandHandler(
            _shoppingCartRepositoryMock.Object,
            _productRepositoryMock.Object,
            _contextAccessorMock.Object,
            _supabaseServiceMock.Object,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenShoppingCartDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _contextAccessorMock.Setup(ctx => ctx.HttpContext)
            .Returns(HttpContextMockProvider.GetHttpContext(userId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync((ShoppingCart?)null);

        var command = new UpdateShoppingCartItemCommand(ProductLineItemRequestFaker.Create(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("user have not created a Shopping cart ");
        result.Errors.Single().Code.Should().Be("ShoppingCart");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenCartItemDoesNotExist()
    {
        // Arrange
        var cart = ShoppingCartFaker.Create();

        _contextAccessorMock.Setup(ctx => ctx.HttpContext)
            .Returns(HttpContextMockProvider.GetHttpContext(cart.UserId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(cart.UserId))
            .ReturnsAsync(cart);

        var command = new UpdateShoppingCartItemCommand(ProductLineItemRequestFaker.Create(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Item not found in cart");
        result.Errors.Single().Code.Should().Be("ShoppingCartItem");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = ShoppingCartFaker.Create();
        cart.UserId = userId;
        var existingItemId = cart.Items.First().Id;

        _contextAccessorMock.Setup(ctx => ctx.HttpContext)
            .Returns(HttpContextMockProvider.GetHttpContext(userId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(cart);
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Product?)null);

        var command = new UpdateShoppingCartItemCommand(ProductLineItemRequestFaker.Create(), existingItemId);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Product was deleted");
        result.Errors.Single().Code.Should().Be("Product");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldFailWithBadRequest_WhenVariantDoesNotExistInProduct()
    {
        // Arrange
        var cart = ShoppingCartFaker.Create();
        var existingItemId = cart.Items.First().Id;
        var product = ProductFaker.CreateTestProduct(includeVariants: false);

        _contextAccessorMock.Setup(ctx => ctx.HttpContext)
            .Returns(HttpContextMockProvider.GetHttpContext(cart.UserId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(cart.UserId))
            .ReturnsAsync(cart);
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(product);

        var command = new UpdateShoppingCartItemCommand(ProductLineItemRequestFaker.Create(includeVariants: false), existingItemId);
        command.Item.Variants.Add("NonExistingVariant", "SomeValue");
        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be($"Variant 'NonExistingVariant' not found for this product");
        result.Errors.Single().Code.Should().Be("Product.Variant");
        result.Errors.Single().Type.Should().Be(ErrorType.BadRequest);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenItemIsUpdatedSuccessfully()
    {
        // Arrange
        var cart = ShoppingCartFaker.Create();
        var existingItemId = cart.Items.First().Id;
        var product = ProductFaker.CreateTestProduct(includeVariants: false);
        var updatedItemRequest = ProductLineItemRequestFaker.Create(includeVariants: false);

        _contextAccessorMock.Setup(ctx => ctx.HttpContext)
            .Returns(HttpContextMockProvider.GetHttpContext(cart.UserId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(cart.UserId))
            .ReturnsAsync(cart);
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(updatedItemRequest.ProductId))
            .ReturnsAsync(product);
        _supabaseServiceMock.Setup(s => s.GetPublicUrl(SupabaseBackets.Products, product.PrimaryImage))
            .Returns("test-url");

        var command = new UpdateShoppingCartItemCommand(updatedItemRequest, existingItemId);

        // Act
        var result = await _handler.Handle(command, default);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value?.Quantity.Should().Be(updatedItemRequest.Quantity);
        result.Value?.Name.Should().Be(product.Name);
        result.Value?.UnitPrice.Ammount.Should().Be(product.Price.Ammount);
        result.Value?.Image.Should().Be("test-url");
    }
}

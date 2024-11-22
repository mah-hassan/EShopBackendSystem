using EShop.Application.ShoppingCarts.Commands.RemoveItemFromCart;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ShoppingCarts;
using EShop.Test.SharedUtilities.ShoppingCarts;
using EShop.Test.SharedUtilities;
using Microsoft.AspNetCore.Http;
using Moq;
using FluentAssertions;

namespace EShop.Test.Application.ShoppingCarts.Commands.RemoveShoppingCartItem;

public class RemoveItemFromCartCommandHandlerTests
{
    private readonly Mock<IShoppingCartRepository> _shoppingCartRepositoryMock = new();
    private readonly Mock<IHttpContextAccessor> _contextAccessorMock = new();
    private readonly RemoveItemFromCartCommandHandler _handler;

    public RemoveItemFromCartCommandHandlerTests()
    {
        _handler = new RemoveItemFromCartCommandHandler(
            _shoppingCartRepositoryMock.Object,
            _contextAccessorMock.Object);
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

        // Act
        var result = await _handler.Handle(new RemoveItemFromCartCommand(Guid.NewGuid()), default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Shopping cart not found");
        result.Errors.Single().Code.Should().Be("ShoppingCart");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenItemDoesNotExistInCart()
    {
        // Arrange
        var cart = ShoppingCartFaker.Create();
        var nonExistingItemId = Guid.NewGuid();

        _contextAccessorMock.Setup(ctx => ctx.HttpContext)
            .Returns(HttpContextMockProvider.GetHttpContext(cart.UserId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(cart.UserId))
            .ReturnsAsync(cart);

        // Act
        var result = await _handler.Handle(new RemoveItemFromCartCommand(nonExistingItemId), default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Item not found in shopping cart");
        result.Errors.Single().Code.Should().Be("ShoppingCartItem");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenItemIsRemovedSuccessfully()
    {
        // Arrange
        var cart = ShoppingCartFaker.Create();

        var itemToRemove = cart.Items.First();

        _contextAccessorMock.Setup(ctx => ctx.HttpContext)
            .Returns(HttpContextMockProvider.GetHttpContext(cart.UserId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(cart.UserId))
            .ReturnsAsync(cart);
        _shoppingCartRepositoryMock.Setup(repo => repo.CreateOrUpdateAsync(cart))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(new RemoveItemFromCartCommand(itemToRemove.Id), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().NotContain(itemToRemove);
    }
}

using EShop.Application.ShoppingCarts.Commands.DeleteShoppingCart;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ShoppingCarts;
using EShop.Test.SharedUtilities.ShoppingCarts;
using EShop.Test.SharedUtilities;
using Microsoft.AspNetCore.Http;
using Moq;
using FluentAssertions;

namespace EShop.Test.Application.ShoppingCarts.Commands.DeleteShoppingCart;

public class DeleteShoppingCartCommandHandlerTests
{
    private readonly Mock<IShoppingCartRepository> _shoppingCartRepositoryMock = new();
    private readonly Mock<IHttpContextAccessor> _contextAccessorMock = new();
    private readonly DeleteShoppingCartCommandHandler _handler;

    public DeleteShoppingCartCommandHandlerTests()
    {
        _handler = new DeleteShoppingCartCommandHandler(
            _contextAccessorMock.Object,
            _shoppingCartRepositoryMock.Object);
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
        var result = await _handler.Handle(new DeleteShoppingCartCommand(), default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Shopping cart not found");
        result.Errors.Single().Code.Should().Be("ShoppingCart");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldFailWithInternalServerError_WhenDeleteOperationFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shoppingCart = ShoppingCartFaker.Create();
        shoppingCart.UserId = userId;

        _contextAccessorMock.Setup(ctx => ctx.HttpContext)
            .Returns(HttpContextMockProvider.GetHttpContext(userId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(shoppingCart);
        _shoppingCartRepositoryMock.Setup(repo => repo.DeleteAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(new DeleteShoppingCartCommand(), default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Failed to delete shopping cart");
        result.Errors.Single().Code.Should().Be("ShoppingCart");
        result.Errors.Single().Type.Should().Be(ErrorType.InternalServerError);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenShoppingCartIsDeletedSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shoppingCart = ShoppingCartFaker.Create();
        shoppingCart.UserId = userId;

        _contextAccessorMock.Setup(ctx => ctx.HttpContext)
            .Returns(HttpContextMockProvider.GetHttpContext(userId));
        _shoppingCartRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(shoppingCart);
        _shoppingCartRepositoryMock.Setup(repo => repo.DeleteAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(new DeleteShoppingCartCommand(), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
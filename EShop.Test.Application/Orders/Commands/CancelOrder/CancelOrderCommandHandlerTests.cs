namespace EShop.Test.Application.Orders.Commands.CancelOrder;

using Xunit;
using Moq;
using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using EShop.Application.Abstractions.Services;
using EShop.Application.Abstractions;
using EShop.Application.Orders.Commands.CancelOrder;
using EShop.Domain.Orders;
using EShop.Domain.Shared.Errors;
using EShop.Test.SharedUtilities.Orders;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IStripeService> _stripeServiceMock = new();
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _handler = new CancelOrderCommandHandler(
            _orderRepositoryMock.Object,
            _stripeServiceMock.Object,
            _eventBusMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var command = new CancelOrderCommand(Guid.NewGuid());
        _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Order not found");
        result.Errors.Single().Code.Should().Be("Order");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Theory]
    [InlineData(OrderStatus.Canceled)]
    [InlineData(OrderStatus.Completed)]
    public async Task Handle_ShouldFailWithConflict_WhenOrderAlreadyCanceledOrCompleted(OrderStatus status)
    {
        // Arrange
        var order = OrderFacker.CreateTestOrder(status);
        var command = new CancelOrderCommand(order.Id);
        _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(order.Id)).ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be($"Order is already {status}");
        result.Errors.Single().Code.Should().Be("Order");
        result.Errors.Single().Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_ShouldRefund_WhenOrderHasPaymentId()
    {
        // Arrange
        var order = OrderFacker.CreateTestOrder(OrderStatus.Placed);
        order.PaymentId = Guid.NewGuid().ToString();
        var command = new CancelOrderCommand(order.Id);

        _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(order.Id)).ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _stripeServiceMock.Verify(s => s.RefundAsync(order.PaymentId), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishEvent_WhenOrderIsCanceled()
    {
        // Arrange
        var order = OrderFacker.CreateTestOrder(OrderStatus.Placed);
        var command = new CancelOrderCommand(order.Id);

        _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(order.Id)).ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _eventBusMock.Verify(e => e.PublishAsync(It.Is<OrderCanceledEvent>(evt =>
            evt.OrderId == order.Id &&
            evt.StatusBeforeCanceling == OrderStatus.Placed &&
            evt.Items == order.Items &&
            evt.ShippingInfo == order.ShippingInfo
        )), Times.Once);
    }
}

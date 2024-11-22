using EShop.Application.Abstractions.Services;
using EShop.Application.Orders.Commands.Checkout;
using EShop.Domain.Invoices;
using EShop.Domain.Orders;
using EShop.Domain.Shared.Errors;
using EShop.Test.SharedUtilities.Orders;
using FluentAssertions;
using Moq;

namespace EShop.Test.Application.Orders.Commands.StartCheckout;

public sealed class StartOrderCheckoutSessionCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<ICouponRepository> _couponRepositoryMock = new();
    private readonly Mock<IStripeService> _stripeServiceMock = new();
    private readonly StartOrderCheckoutSessionCommandHandler _handler;

    public StartOrderCheckoutSessionCommandHandlerTests()
    {
        _handler = new StartOrderCheckoutSessionCommandHandler(
            _orderRepositoryMock.Object,
            _couponRepositoryMock.Object,
            _stripeServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var command = new StartOrderCheckoutSessionCommand(Guid.NewGuid());
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(command.Id))
                            .ReturnsAsync(default(Order));

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Order Not Found");
        result.Errors.Single().Code.Should().Be("Order");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldFailWithBadRequest_WhenOrderStatusIsNotPlaced()
    {
        // Arrange
        var order = OrderFacker.CreateTestOrder(OrderStatus.Shipped); // Assume shipped for invalid status
        var command = new StartOrderCheckoutSessionCommand(order.Id);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(command.Id))
                            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be($"Order have been {order.Status}");
        result.Errors.Single().Code.Should().Be("Order.Status");
        result.Errors.Single().Type.Should().Be(ErrorType.BadRequest);
    }

    [Fact]
    public async Task Handle_ShouldReturnSessionUrl_WhenOrderIsValid()
    {
        // Arrange
        var order = OrderFacker.CreateTestOrder(OrderStatus.Placed); // Assume shipped for invalid status
        var command = new StartOrderCheckoutSessionCommand(order.Id);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(command.Id))
                            .ReturnsAsync(order);


        var expectedUrl = "https://stripe.com/session";
        _stripeServiceMock.Setup(s => s.CreatePaymentSessionAsync(order, It.IsAny<string>()))
                            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value?.Should().Be(expectedUrl);
    }
    
}
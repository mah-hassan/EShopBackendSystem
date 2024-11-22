using EShop.Application.Abstractions.Mappers;
using EShop.Application.Abstractions.Services;
using EShop.Application.Abstractions;
using EShop.Application.Orders.Commands.UpdateOrder;
using EShop.Domain.Invoices;
using EShop.Domain.Orders;
using EShop.Domain.Products;
using Moq;
using EShop.Test.SharedUtilities.Orders;
using EShop.Domain.Shared.Errors;
using FluentAssertions;
using EShop.Contracts.Orders;
using DeliveryMethod = EShop.Domain.Orders.DeliveryMethod;
using EShop.Test.SharedUtilities.Products;
using EShop.Test.SharedUtilities.Common;

namespace EShop.Test.Application.Orders.Commands.UpdateOrder
{
    public sealed class UpdateOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
        private readonly Mock<IDeliveryMethodRepository> _deliveryMethodRepositoryMock = new();
        private readonly Mock<IProductRepository> _productRepositoryMock = new();
        private readonly Mock<ICouponRepository> _couponRepositoryMock = new();
        private readonly Mock<ISupabaseService> _supabaseServiceMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mapper _mapper = new();
        private readonly UpdateOrderCommandHandler _handler;

        public UpdateOrderCommandHandlerTests()
        {
            _handler = new UpdateOrderCommandHandler(
                _mapper,
                _orderRepositoryMock.Object,
                _deliveryMethodRepositoryMock.Object,
                _productRepositoryMock.Object,
                _couponRepositoryMock.Object,
                _supabaseServiceMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldFailWithNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var command = new UpdateOrderCommand(Guid.NewGuid(), UpdateOrderRequestFaker.Create());
            _orderRepositoryMock.Setup(r => r.GetByIdAsync(command.Id))
                                .ReturnsAsync(default(Order));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Single().Message.Should().Be("Order not found");
            result.Errors.Single().Code.Should().Be("Order");
            result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Handle_ShouldFailWithBadRequest_WhenOrderStatusIsNotPlaced()
        {
            // Arrange
            var order = OrderFacker.CreateTestOrder(OrderStatus.Shipped);
            var command = new UpdateOrderCommand(order.Id, UpdateOrderRequestFaker.Create());
            _orderRepositoryMock.Setup(r => r.GetByIdAsync(command.Id))
                                .ReturnsAsync(order);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Single().Message.Should().Be($"can not update a {nameof(order.Status)} order");
            result.Errors.Single().Code.Should().Be("Order");
            result.Errors.Single().Type.Should().Be(ErrorType.BadRequest);
        }

        [Fact]
        public async Task Handle_ShouldFailWithNotFound_WhenDeliveryMethodIsInvalid()
        {
            // Arrange
            var order = OrderFacker.CreateTestOrder(OrderStatus.Placed);
            var command = new UpdateOrderCommand(order.Id, UpdateOrderRequestFaker.Create());
            _orderRepositoryMock.Setup(r => r.GetByIdAsync(command.Id))
                                .ReturnsAsync(order);
            _deliveryMethodRepositoryMock.Setup(d => d.GetByIdAsync(command.UpdatedOrder.DeliveryMethodId))
                                         .ReturnsAsync(default(DeliveryMethod));

            // Act
            var result = await _handler.Handle(command, default);
            
            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Single().Message.Should().Be("UnSupported Delivery method");
            result.Errors.Single().Code.Should().Be("DeliveryMethod");
            result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Handle_ShouldFailWithNotFound_WhenProductIsOutOfStock()
        {
            // Arrange
            var product = ProductFaker.CreateTestProduct(stockQuantity: 3, orderedQuantity: 3);

            var updatedItem = ProductLineItemFaker.Create();

            var order = OrderFacker.CreateTestOrder(OrderStatus.Placed)
                .ShouldHasItem(new OrderItem()
                {
                    ProductId = updatedItem.ProductId,
                    UnitPrice = product.Price,
                });
            var deliveryMethod = DeliveryMethodFaker.Create();
            var command = new UpdateOrderCommand(order.Id, new UpdateOrderRequest { DeliveryMethodId = deliveryMethod.Id, Items =  { updatedItem } });

            _orderRepositoryMock.Setup(r => r.GetByIdAsync(command.Id))
                                .ReturnsAsync(order);

            _deliveryMethodRepositoryMock.Setup(d => d.GetByIdAsync(command.UpdatedOrder.DeliveryMethodId))
                             .ReturnsAsync(deliveryMethod);

            _productRepositoryMock.Setup(p => p.GetByIdAsync(updatedItem.ProductId))
                                  .ReturnsAsync(product);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Single().Message.Should().Be($"{product.Name} is out of stock");
            result.Errors.Single().Code.Should().Be("Product");
            result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
        }



        [Fact]
        public async Task Handle_ShouldUpdateOrderSuccessfully_WhenOrderIsValid()
        {
            // Arrange
            var product = ProductFaker.CreateTestProduct();
            var updatedItem = ProductLineItemFaker.Create();
            var order = OrderFacker.CreateTestOrder(OrderStatus.Placed)
                .ShouldHasItem(new OrderItem()
                {
                    ProductId = updatedItem.ProductId,
                    UnitPrice = product.Price,
                });

            var deliveryMethod = DeliveryMethodFaker.Create();
            var command = new UpdateOrderCommand(order.Id, new UpdateOrderRequest
            {
                DeliveryMethodId = deliveryMethod.Id,
                ShippingInfo = ShippingInfoRquestFaker.Create(),
                Items = { updatedItem }
            });

            _orderRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(order);
            _deliveryMethodRepositoryMock.Setup(d => d.GetByIdAsync(command.UpdatedOrder.DeliveryMethodId))
                                         .ReturnsAsync(deliveryMethod);
            _productRepositoryMock.Setup(p => p.GetByIdAsync(updatedItem.ProductId))
                                  .ReturnsAsync(product);
            _supabaseServiceMock.Setup(s => s.GetPublicUrl(It.IsAny<string>(), product.PrimaryImage))
                                .Returns("https://supabase.com/product-image");
            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _orderRepositoryMock.Verify(r => r.Update(It.Is<Order>(o =>
                            o.Id == command.Id &&
                            o.DeliveryMethodId == deliveryMethod.Id &&
                            o.Items.Any(i => i.ProductId == updatedItem.ProductId &&
                            i.UnitPrice == product.Price && i.Quantity == updatedItem.Quantity))), Times.Once);

            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }


    }
}
using EShop.Application.Abstractions.Mappers;
using EShop.Application.Abstractions;
using EShop.Application.Orders.Commands.CreateOrder;
using EShop.Domain.Invoices;
using EShop.Domain.Orders;
using EShop.Domain.Products;
using EShop.Domain.ShoppingCarts;
using Microsoft.AspNetCore.Http;
using Moq;
using EShop.Contracts.Orders;
using FluentAssertions;
using EShop.Domain.Shared.Errors;
using EShop.Test.SharedUtilities;
using DeliveryMethod = EShop.Domain.Orders.DeliveryMethod;
using EShop.Test.SharedUtilities.Products;
using EShop.Test.SharedUtilities.Orders;

namespace EShop.Test.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<ICouponRepository> _couponRepositoryMock = new();
    private readonly Mock<IShoppingCartRepository> _shoppingCartRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IHttpContextAccessor> _contextAccessorMock = new();
    private readonly Mock<IDeliveryMethodRepository> _deliveryMethodRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mapper _mapper = new();


    private CreateOrderCommandHandler CreateHandler()
    {
         return new CreateOrderCommandHandler(
            _mapper,
            _orderRepositoryMock.Object,
            _couponRepositoryMock.Object,
            _shoppingCartRepositoryMock.Object,
            _productRepositoryMock.Object,
            _contextAccessorMock.Object,
            _deliveryMethodRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    private OrderRequest CreateOrderRequest()
    {
        return new OrderRequest
        {
            DeliveryMethodId = Guid.NewGuid(),
            ShippingInfo = new Contracts.Orders.ShippingInfo
            {
                City = "city",
                Region = "region",
                Email = "email",
                FirstName = "firstName",
                LastName = "lastName",
                Phone = "phoneNumber",
                PostalCode = "postalCode",
            },
        };
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenShoppingCartIsNull()
    {
        // Arrange
        _shoppingCartRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(default(ShoppingCart));
        _contextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextMockProvider.GetHttpContext());
        var command = new CreateOrderCommand(CreateOrderRequest());

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
        result.Errors.Single().Code.Should().Be("ShoppingCart");
        result.Errors.Single().Message.Should().Be("User does not have a shopping cart to checkout");
    }

    [Fact]
    public async Task Handle_ShouldFailWithBadRequest_WhenShoppingCartIsEmpty()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        _contextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(userId));
        

        var emptyCart = new ShoppingCart 
        {
            UserId = userId,
            Items = new() 
        };

        _shoppingCartRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(emptyCart);

        var command = new CreateOrderCommand(CreateOrderRequest());

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Type.Should().Be(ErrorType.BadRequest);
        result.Errors.Single().Code.Should().Be("ShoppingCart");
        result.Errors.Single().Message.Should().Be("can not create order, Shopping cart is empty");

    }


    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenDeliveryMethodIsNull()
    {
        // Arrange
        OrderRequest order = CreateOrderRequest();
        Guid userId = Guid.NewGuid();

        _contextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(userId));


        var cart = new ShoppingCart
        {
            UserId = userId,
            Items = new()
            {
                new()
            },
        };

        _shoppingCartRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(cart);

        _deliveryMethodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(order.DeliveryMethodId))
            .ReturnsAsync(default(DeliveryMethod));

        var command = new CreateOrderCommand(order);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
        result.Errors.Single().Code.Should().Be("DeliveryMethod");
        result.Errors.Single().Message.Should().Be("UnSupported Delivery method");
    }

    [Fact]
    public async Task Handle_ShouldFailWithBadRequest_WhenProductOutOfStock()
    {
        // Arrange
        OrderRequest order = CreateOrderRequest();

        Guid userId = Guid.NewGuid();

        _contextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(userId));

        var product = ProductFaker.CreateTestProduct(stockQuantity: 4, orderedQuantity: 4);
        var cart = new ShoppingCart 
        { 
            Items = new List<ShoppingCartItem> { 
                new() 
                { 
                    ProductId = product.Id,
                    Quantity = 5 
                } 
            } ,
            UserId = userId,

        };

        _shoppingCartRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(cart);

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        _deliveryMethodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(order.DeliveryMethodId))
            .ReturnsAsync(DeliveryMethodFaker.Create());

        var command = new CreateOrderCommand(order);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Type.Should().Be(ErrorType.BadRequest);
        result.Errors.Single().Code.Should().Be("Product");

        result.Errors.Single().Message.Should().Be($"{product.Name} is out of stock");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenOrderIsCreatedSuccessfully()
    {
        // Arrange
        OrderRequest order = CreateOrderRequest();

        Guid userId = Guid.NewGuid();

        _contextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextMockProvider.GetHttpContext(userId));

        var product = ProductFaker.CreateTestProduct();
        var cart = new ShoppingCart
        {
            Items = new List<ShoppingCartItem> { 
                new()
                {
                    ProductId = product.Id,
                    Quantity = 5
                }
            },
            UserId = userId,

        };

        _shoppingCartRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(cart);

        _productRepositoryMock
            .Setup(repo => repo.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        _deliveryMethodRepositoryMock
            .Setup(repo => repo.GetByIdAsync(order.DeliveryMethodId))
            .ReturnsAsync(DeliveryMethodFaker.Create());

        var command = new CreateOrderCommand(order);

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value?.Id.Should().NotBeEmpty();
        result.Value?.Status.Should().Be(nameof(OrderStatus.Placed));
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.Add(It.Is<Order>(o => o.Id == result.Value!.Id)), Times.Once);
    }
}
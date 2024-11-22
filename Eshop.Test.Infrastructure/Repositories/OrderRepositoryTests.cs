
using EShop.Domain.Orders;
using EShop.Infrastructure.Repositories;
using Eshop.Test.Infrastructure.Data;
using EShop.Test.SharedUtilities.Orders;
using EShop.Test.SharedUtilities.Products;
using FluentAssertions;

namespace Eshop.Test.Infrastructure.Repositories;

public sealed class OrderRepositoryTests
{
    private readonly TestingDbContext _dbContext;
    private readonly OrderRepository _sut;

    public OrderRepositoryTests()
    {
        _dbContext = new TestingDbContext();

        _dbContext.InitializeAsync().GetAwaiter().GetResult();
        _sut = new OrderRepository(_dbContext);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var orders = OrderFacker.CreateList(5);
        _dbContext.Orders.AddRange(orders);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetAllByCustomerEmailAsync_ShouldReturnOrdersForGivenCustomerEmail()
    {
        // Arrange
        var testEmail = "test@example.com";
        var testOrder = OrderFacker.CreateTestOrder();
        testOrder.CustomerEmail = testEmail;

        _dbContext.Orders.Add(testOrder);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllByCustomerEmailAsync(testEmail);

        // Assert
        result.Should().HaveCount(1);
        result.First().Should().BeEquivalentTo(testOrder); 
    }

    [Fact]
    public async Task RemoveProductFromOrdersAsync_ShouldRemoveOrderItemsWithGivenProductId()
    {
        // Arrange
        var testProduct = ProductFaker.CreateTestProduct();
        var testOrder = OrderFacker.CreateTestOrder()
            .ShouldHasItem(new OrderItem
            {
                ProductId = testProduct.Id,
                Quantity = 1,
                UnitPrice = testProduct.Price,
                Image = testProduct.PrimaryImage,
                Name = testProduct.Name,
            });

        _dbContext.Orders.Add(testOrder);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.RemoveProductFromOrdersAsync(testProduct.Id);

        // Assert
        var orderItems = _dbContext.Set<OrderItem>()
            .Where(oi => oi.ProductId == testProduct.Id)
            .ToList();

        orderItems.Should().BeEmpty();
    }
}

using Moq;
using StackExchange.Redis;
using Newtonsoft.Json;
using FluentAssertions;
using EShop.Infrastructure.Repositories;
using EShop.Test.SharedUtilities.ShoppingCarts;


namespace Eshop.Test.Infrastructure.Repositories;
public class ShoppingCartRepositoryTests
{
    private readonly Mock<IDatabase> _databaseMock;
    private readonly ShoppingCartRepository _sut;

    public ShoppingCartRepositoryTests()
    {
        var redisMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_databaseMock.Object);

        _sut = new ShoppingCartRepository(redisMock.Object);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_ShouldStoreShoppingCartInRedis()
    {
        // Arrange
        var cart = ShoppingCartFaker.Create();
        var key = cart.UserId.ToString();
        var data = JsonConvert.SerializeObject(cart);

        // Act
        await _sut.CreateOrUpdateAsync(cart);

        // Assert
        _databaseMock.Verify(db => db.StringSetAsync(
         key,
         data,
         It.Is<TimeSpan>(t => t.TotalDays == 30),  
         false,
         When.Always,  // Explicitly matches `When.Always`
         It.Is<CommandFlags>(flags => flags == CommandFlags.None)  // Ensures CommandFlags.None
     ), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenKeyExistsAndIsDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var key = userId.ToString();
        _databaseMock.Setup(db => db.KeyDeleteAsync(key, It.IsAny<CommandFlags>())).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(userId);

        // Assert
        result.Should().BeTrue();
        _databaseMock.Verify(db => db.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnShoppingCart_WhenKeyExists()
    {
        // Arrange
        var cart = ShoppingCartFaker.Create();
        var key = cart.UserId.ToString();
        var data = JsonConvert.SerializeObject(cart);
        _databaseMock.Setup(db => db.KeyExistsAsync(key, It.IsAny<CommandFlags>())).ReturnsAsync(true);
        _databaseMock.Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>())).ReturnsAsync(data);

        // Act
        var result = await _sut.GetByUserIdAsync(cart.UserId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cart);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var key = userId.ToString();
        _databaseMock.Setup(db => db.KeyExistsAsync(key, It.IsAny<CommandFlags>())).ReturnsAsync(false);

        // Act
        var result = await _sut.GetByUserIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }
}

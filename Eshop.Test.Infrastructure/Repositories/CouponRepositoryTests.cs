using EShop.Domain.Invoices;
using EShop.Infrastructure.Repositories;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;
namespace Eshop.Test.Infrastructure.Repositories;
public sealed class CouponRepositoryTests
{
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly CouponRepository _sut;

    public CouponRepositoryTests()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_databaseMock.Object);
        _sut = new CouponRepository(_redisMock.Object);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldStoreCouponAndAddToSet()
    {
        // Arrange
        var coupon = new Coupon { Code = "SAVE10", SavePercentage = 10, MinimumAmount = 100 };
        var payload = JsonConvert.SerializeObject(coupon);

        // Act
        await _sut.AddOrUpdateAsync(coupon);

        // Assert
        _databaseMock.Verify(db => db.StringSetAsync(coupon.Code, payload, null,false, When.Always, CommandFlags.None), Times.Once);
        _databaseMock.Verify(db => db.SetAddAsync("coupons", coupon.Code, CommandFlags.None), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveCouponAndRemoveFromSet()
    {
        // Arrange
        var code = "SAVE10";
        _databaseMock.Setup(db => db.KeyDeleteAsync(code, CommandFlags.None)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(code);

        // Assert
        result.Should().BeTrue();
        _databaseMock.Verify(db => db.SetRemoveAsync("coupons", code, CommandFlags.None), Times.Once);
        _databaseMock.Verify(db => db.KeyDeleteAsync(code, CommandFlags.None), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCoupons()
    {
        // Arrange
        var coupon1 = new Coupon { Code = "SAVE10", SavePercentage = 10, MinimumAmount = 100 };
        var coupon2 = new Coupon { Code = "SAVE20", SavePercentage = 20, MinimumAmount = 200 };

        var payload1 = JsonConvert.SerializeObject(coupon1);
        var payload2 = JsonConvert.SerializeObject(coupon2);

        _databaseMock.Setup(db => db.SetMembersAsync("coupons", CommandFlags.None))
            .ReturnsAsync(new RedisValue[] { coupon1.Code, coupon2.Code });

        _databaseMock.Setup(db => db.StringGetAsync(coupon1.Code, CommandFlags.None)).ReturnsAsync(payload1);
        _databaseMock.Setup(db => db.StringGetAsync(coupon2.Code, CommandFlags.None)).ReturnsAsync(payload2);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(coupon1);
        result.Should().ContainEquivalentOf(coupon2);
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnCoupon_WhenCouponExists()
    {
        // Arrange
        var coupon = new Coupon { Code = "SAVE10", SavePercentage = 10, MinimumAmount = 100 };
        var payload = JsonConvert.SerializeObject(coupon);

        _databaseMock.Setup(db => db.StringGetAsync(coupon.Code, CommandFlags.None)).ReturnsAsync(payload);

        // Act
        var result = await _sut.GetByCodeAsync(coupon.Code);

        // Assert
        result.Should().BeEquivalentTo(coupon);
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnNull_WhenCouponDoesNotExist()
    {
        // Arrange
        var code = "SAVE10";
        _databaseMock.Setup(db => db.StringGetAsync(code, CommandFlags.None)).ReturnsAsync((RedisValue)RedisValue.Null);

        // Act
        var result = await _sut.GetByCodeAsync(code);

        // Assert
        result.Should().BeNull();
    }
}

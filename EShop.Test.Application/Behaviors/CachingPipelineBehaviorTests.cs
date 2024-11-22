using EShop.Application.Abstractions.Messaging;
using EShop.Application.Abstractions.Services;
using EShop.Application.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace EShop.Test.Application.Behaviors;

public class CachingPipelineBehaviorTests
{
    private readonly Mock<ILogger<CachingPipelineBehavior<TestQuery, TestResponse>>> _loggerMock = new();
    private readonly Mock<ICachService> _cacheServiceMock = new();
    private readonly Mock<RequestHandlerDelegate<TestResponse>> _nextMock = new();
    private readonly CachingPipelineBehavior<TestQuery, TestResponse> _cachingBehavior;

    public CachingPipelineBehaviorTests()
    {
        _cachingBehavior = new CachingPipelineBehavior<TestQuery, TestResponse>(
            _loggerMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCachedResponse_WhenCacheIsAvailable()
    {
        // Arrange
        var query = new TestQuery { CachKey = "test-key" };
        var cachedResponse = new TestResponse();
        _cacheServiceMock.Setup(s => s.GetAsync<TestResponse>(query.CachKey))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _cachingBehavior.Handle(query, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(cachedResponse);
        _cacheServiceMock.Verify(s => s.GetAsync<TestResponse>(query.CachKey), Times.Once);
        _nextMock.Verify(next => next(), Times.Never); // Ensure handler isn't called when cache exists
    }

    [Fact]
    public async Task Handle_ShouldCallHandlerAndCacheResult_WhenCacheIsNotAvailable()
    {
        // Arrange
        var query = new TestQuery { CachKey = "test-key" };
        var response = new TestResponse();
        _cacheServiceMock.Setup(s => s.GetAsync<TestResponse>(query.CachKey))
            .ReturnsAsync((TestResponse?)null); // No cache available
        _nextMock.Setup(next => next())
            .ReturnsAsync(response);

        // Act
        var result = await _cachingBehavior.Handle(query, _nextMock.Object, default);

        // Assert
        result.Should().Be(response);
        _nextMock.Verify(next => next(), Times.Once); // Ensure handler is called when cache doesn't exist
        _cacheServiceMock.Verify(s => s.AddOrUpdateAsync(response, query.CachKey, query.Period), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogError_WhenCacheRetrievalFails()
    {
        // Arrange
        var query = new TestQuery { CachKey = "test-key" };
        var response = new TestResponse();
        _cacheServiceMock.Setup(s => s.GetAsync<TestResponse>(query.CachKey))
            .ThrowsAsync(new Exception("Cache retrieval failed"));
        _nextMock.Setup(next => next())
            .ReturnsAsync(response);

        string logMessage = $"An error occurred while retrieving cache for {typeof(TestQuery)} with key: {query.CachKey}";

        // Act
        var result = await _cachingBehavior.Handle(query, _nextMock.Object, default);

        // Assert
        result.Should().Be(response);
        _nextMock.Verify(next => next(), Times.Once);
        _loggerMock.Verify(
            l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Equals(logMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

    }

    [Fact]
    public async Task Handle_ShouldLogError_WhenCacheUpdateFails()
    {
        // Arrange
        var query = new TestQuery { CachKey = "test-key" };
        var response = new TestResponse();
        _cacheServiceMock.Setup(s => s.GetAsync<TestResponse>(query.CachKey))
            .ReturnsAsync((TestResponse?)null);
        _nextMock.Setup(next => next())
            .ReturnsAsync(response);
        _cacheServiceMock.Setup(s => s.AddOrUpdateAsync(response, query.CachKey, query.Period))
            .ThrowsAsync(new Exception("Cache update failed"));
        string logMessage = $"An error occurred while adding or updating cache for {typeof(TestQuery)} with key: {query.CachKey}";
        // Act
        var result = await _cachingBehavior.Handle(query, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(response);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Equals(logMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

public class TestQuery : ICachedQuery
{
    public required string CachKey { get; set; }

    public TimeSpan? Period => TimeSpan.FromMinutes(1);
}

public class TestResponse;
using Moq;
using FluentAssertions;
using EShop.Application.Abstractions.Services;
using EShop.Application.Abstractions;
using EShop.Application.Brands.Commands.DeleteBrand;
using EShop.Domain.Brands;
using EShop.Domain.Shared.Errors;
using EShop.Test.SharedUtilities.Brands;

namespace EShop.Test.Application.Brands.Commands.DeleteBrand;

public class DeleteBrandCommandHandlerTests
{
    private readonly Mock<IBrandRepository> _brandRepositoryMock = new();
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DeleteBrandCommandHandler _handler;

    public DeleteBrandCommandHandlerTests()
    {
        _handler = new DeleteBrandCommandHandler(
            _brandRepositoryMock.Object,
            _eventBusMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenBrandDoesNotExist()
    {
        // Arrange
        var command = new DeleteBrandCommand(Guid.NewGuid());
        _brandRepositoryMock.Setup(repo => repo.GetByIdAsync(command.brandId))
            .ReturnsAsync(default(Brand));

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Brand not found");
        result.Errors.Single().Code.Should().Be("Brand");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenBrandIsAlreadyDeleted()
    {
        // Arrange
        var brand = BrandFaker.Create(true);
        var command = new DeleteBrandCommand(brand.Id);
        _brandRepositoryMock.Setup(repo => repo.GetByIdAsync(command.brandId)).ReturnsAsync(brand);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Brand not found");
        result.Errors.Single().Code.Should().Be("Brand");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldSucceedAndPublishEvent_WhenBrandIsDeletedSuccessfully()
    {
        // Arrange
        var brand = BrandFaker.Create();
        var command = new DeleteBrandCommand(brand.Id);

        _brandRepositoryMock.Setup(repo => repo.GetByIdAsync(command.brandId)).ReturnsAsync(brand);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _brandRepositoryMock.Verify(repo => repo.Update(brand), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBusMock.Verify(e => e.PublishAsync(It.Is<BrandDeletedEvent>(evt => evt.BrandId == brand.Id)), Times.Once);
    }
}

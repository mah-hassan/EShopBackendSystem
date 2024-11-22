using EShop.Application.Abstractions;
using EShop.Application.Abstractions.Services;
using EShop.Application.Products.Commands.DeleteProduct;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using FluentAssertions;
using Moq;
using EShop.Test.SharedUtilities.Products;

namespace EShop.Test.Application.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventBus> _eventBusMock;

    public DeleteProductCommandHandlerTests()
    {
        _productRepositoryMock = new();
        _unitOfWorkMock = new();
        _eventBusMock = new();
    }

    private DeleteProductCommandHandler CreateHandler()
    {
        return new DeleteProductCommandHandler(
            _productRepositoryMock.Object,
            _eventBusMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var command = new DeleteProductCommand(Guid.NewGuid());
        var handler = CreateHandler();

        _productRepositoryMock.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync(default(Product));

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Product");
        result.Errors.Single().Message.Should().Be("Product not found");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
        _productRepositoryMock.Verify(x => x.Delete(It.IsAny<Product>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ProductDeletedEvent>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteProduct_AndPublishEvent()
    {
        // Arrange
        var product = ProductFaker.CreateTestProduct();

        var command = new DeleteProductCommand(product.Id);
        var handler = CreateHandler();

        _productRepositoryMock.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync(product);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _productRepositoryMock.Verify(x => x.Delete(product), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<ProductDeletedEvent>(e =>
            e.ProductId == product.Id &&
            e.ProductName == product.Name &&
            e.ProductBrandId == product.BrandId &&
            e.ProductCategoryId == product.CategoryId &&
            e.PrimaryImage == product.PrimaryImage)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotPublishEvent_WhenDeletionFails()
    {
        // Arrange
        var product = ProductFaker.CreateTestProduct();

        var command = new DeleteProductCommand(product.Id);
        var handler = CreateHandler();

        _productRepositoryMock.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = () => handler.Handle(command, default);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ProductDeletedEvent>()), Times.Never);
    }
}
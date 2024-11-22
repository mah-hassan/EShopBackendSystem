using EShop.Application.Abstractions;
using EShop.Application.Abstractions.Mappers;
using EShop.Application.Abstractions.Services;
using EShop.Application.Common.Constants;
using EShop.Application.Products.Commands.UpdateProduct;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using EShop.Test.SharedUtilities.Products;

namespace EShop.Test.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductAttribuatesRepository> _productAttribuatesRepositoryMock;
    private readonly Mock<ISupabaseService> _supabaseServiceMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly Mapper mapper;


    public UpdateProductCommandHandlerTests()
    {
        _productRepositoryMock = new();
        _productAttribuatesRepositoryMock = new();
        _supabaseServiceMock = new();
        _eventBusMock = new();
        mapper = new();
        _unitOfWorkMock = new();

    }
    private UpdateProductCommandHandler CreateHandler()
    {
        return new UpdateProductCommandHandler(
                    _productRepositoryMock.Object,
                    _productAttribuatesRepositoryMock.Object,
                    _supabaseServiceMock.Object,
                    _eventBusMock.Object,
                    _unitOfWorkMock.Object,
                    mapper);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenProductIsNull()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var command = new UpdateProductCommand(id, UpdateProductRequestFaker.CreateUpdateProductRequest());
        var handler = CreateHandler();
        // setup product repository
        _productRepositoryMock.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync(default(Product));
        // Act
        var result = await handler.Handle(command, default);
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Product");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
        result.Errors.Single().Message.Should().Be("Product not found");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ProductUpdatedEvent>()), Times.Never);
    }


    [Fact]
    public async Task Handle_ShouldUploadNewPrimaryImage_AndDeleteOldOne()
    {
        // Arrange
        var exsistingProduct = ProductFaker.CreateTestProduct();
        var oldPrimaryImage = exsistingProduct.PrimaryImage;
        var command = new UpdateProductCommand(exsistingProduct.Id, UpdateProductRequestFaker.CreateUpdateProductRequest());
        var handler = CreateHandler();
        var newPrimaryImage = $"product-{command.Id}{Path.GetExtension(command.UpdatedProduct.PrimaryImage!.FileName)}";

        _productRepositoryMock.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync(exsistingProduct);

        _supabaseServiceMock.Setup(x => x.UploadAsync(It.IsAny<IFormFile>(), SupabaseBackets.Products,
            newPrimaryImage)).ReturnsAsync($"https://fthzxnctwbfonupfgquu.supabase.co/storage/v1/object/public/{SupabaseBackets.Products}/{newPrimaryImage}");

        _productAttribuatesRepositoryMock.Setup(repo => repo.GetProductAttributesAsync(command.Id))
            .ReturnsAsync(new List<ProductAttribuates>());

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _supabaseServiceMock.Verify(x => x.UploadAsync(command.UpdatedProduct.PrimaryImage!, SupabaseBackets.Products, It.IsAny<string>()), Times.Once);

        _supabaseServiceMock.Verify(x => x.DeleteFileAsync(SupabaseBackets.Products, oldPrimaryImage), Times.Once);
        exsistingProduct.PrimaryImage.Should().Be($"https://fthzxnctwbfonupfgquu.supabase.co/storage/v1/object/public/{SupabaseBackets.Products}/{newPrimaryImage}");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ProductUpdatedEvent>()), Times.Once);

    }


    [Fact]
    public async Task Handle_ShouldNotAddExistingVariantOptions_WhenOptionsAlreadyExist()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var command = new UpdateProductCommand(id, UpdateProductRequestFaker.CreateUpdateProductRequest());
        var handler = CreateHandler();
        var exsistingProduct = ProductFaker.CreateTestProduct();
        List<ProductAttribuates> existingAttributes = command.UpdatedProduct
            .Attribuates.Select(x => new ProductAttribuates
            {
                ProductId = exsistingProduct.Id,
                VariantOptionId = x
            }).ToList();

        _productRepositoryMock.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync(exsistingProduct);

        _productAttribuatesRepositoryMock.Setup(x => x.GetProductAttributesAsync(exsistingProduct.Id)).ReturnsAsync(existingAttributes);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _productAttribuatesRepositoryMock.Verify(x => x.AddProductAttribuate(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<ProductUpdatedEvent>()), Times.Once);
    }

}
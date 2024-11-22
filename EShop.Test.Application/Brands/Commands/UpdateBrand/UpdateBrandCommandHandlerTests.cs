using Moq;
using FluentAssertions;
using EShop.Application.Abstractions.Mappers;
using EShop.Application.Abstractions.Services;
using EShop.Application.Abstractions;
using EShop.Application.Brands.Commands.UpdateBrand;
using EShop.Application.Common.Constants;
using EShop.Contracts.Brand;
using EShop.Domain.Brands;
using EShop.Domain.Shared.Errors;
using EShop.Test.SharedUtilities.Brands;

namespace EShop.Test.Application.Brands.Commands.UpdateBrand;

public class UpdateBrandCommandHandlerTests
{
    private readonly Mock<IBrandRepository> _brandRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ISupabaseService> _supabaseServiceMock = new();
    private readonly Mapper _mapper = new();
    private readonly UpdateBrandCommandHandler _handler;
    private BrandRequest _brandRequest = BrandRequestFaker.Create();
    public UpdateBrandCommandHandlerTests()
    {
        _handler = new UpdateBrandCommandHandler(
            _brandRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _supabaseServiceMock.Object,
            _mapper);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenBrandDoesNotExist()
    {
        // Arrange
        var command = new UpdateBrandCommand(Guid.NewGuid(), _brandRequest);
        _brandRepositoryMock.Setup(repo => repo.GetByIdAsync(command.brandId))
            .ReturnsAsync((Brand?)null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Message.Should().Be("Brand not found");
        result.Errors.Single().Code.Should().Be("Brand");
        result.Errors.Single().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldUpdateBrandDetails_WhenBrandExistsAndImageIsNotProvided()
    {
        // Arrange
        var brand = BrandFaker.Create();
        var request = _brandRequest;
        request.Image = null;
        var command = new UpdateBrandCommand(brand.Id, request);

        _brandRepositoryMock.Setup(repo => repo.GetByIdAsync(command.brandId)).ReturnsAsync(brand);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value?.Name.Should().Be(_brandRequest.Name);
        result.Value?.Description.Should().Be(_brandRequest.Description);
        _brandRepositoryMock.Verify(repo => repo.Update(brand), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateBrandImage_WhenNewImageIsProvided()
    {
        // Arrange
        var brand = BrandFaker.Create();
        var oldImagePath = brand.Image;
        var newImagePath = $"Brand-{brand.Id}{Path.GetExtension(_brandRequest.Image!.FileName)}";

        var command = new UpdateBrandCommand(brand.Id, _brandRequest);
        _brandRepositoryMock.Setup(repo => repo.GetByIdAsync(command.brandId)).ReturnsAsync(brand);
        _supabaseServiceMock.Setup(s => s.UploadAsync(command.dto.Image!, "Brands", It.IsAny<string>())).ReturnsAsync(newImagePath);
        _supabaseServiceMock.Setup(x => x.GetPublicUrl("Brands", newImagePath))
             .Returns($"https://supabase.com/{newImagePath}");
        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value?.Image.Should().Be($"https://supabase.com/{newImagePath}");
        _supabaseServiceMock.Verify(s => s.DeleteFileAsync(SupabaseBackets.Brands, oldImagePath), Times.Once);
        _supabaseServiceMock.Verify(s => s.UploadAsync(command.dto.Image!, "Brands", newImagePath), Times.Once);
        _brandRepositoryMock.Verify(repo => repo.Update(brand), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

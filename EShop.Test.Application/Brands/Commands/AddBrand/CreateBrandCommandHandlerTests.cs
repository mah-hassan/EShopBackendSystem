using EShop.Application.Abstractions;
using EShop.Application.Abstractions.Mappers;
using EShop.Application.Abstractions.Services;
using EShop.Application.Brands.Commands.AddBrand;
using EShop.Contracts.Brand;
using EShop.Domain.Brands;
using EShop.Domain.Shared.Errors;
using FluentAssertions;
using Moq;
using EShop.Test.SharedUtilities;
using EShop.Test.SharedUtilities.Brands;


namespace EShop.Test.Application.Brands.Commands.AddBrand;

public class CreateBrandCommandHandlerTests
{
    private readonly Mock<IBrandRepository> _brandRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mapper _mapper;
    private readonly Mock<ISupabaseService> _supabaseServiceMock;
    private readonly BrandRequest request = BrandRequestFaker.Create();
    public CreateBrandCommandHandlerTests()
    {
        _brandRepositoryMock = new();
        _unitOfWorkMock = new();
        _mapper = new();
        _supabaseServiceMock = new();
    }

    private CreateBrandCommandHandler CreateHandler()
    {
        return new CreateBrandCommandHandler(
            _mapper,
            _supabaseServiceMock.Object,
            _brandRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }


    [Fact]
    public async Task Handle_WhenBrandAlreadyExists_ReturnsConflictError()
    {
        // Arrange

        _brandRepositoryMock.Setup(repo => repo.IsNameExists(request.Name))
           .ReturnsAsync(true);

        var command = new CreateBrandCommand(request);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors?.First().Type.Should().Be(ErrorType.Conflict);

        // Additional Verifications
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
        _brandRepositoryMock.Verify(x => x.Add(It.IsAny<Brand>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenBrandNameDoesNotExist_ReturnsSuccessResultWithBrandResponse()
    {
        // Arrange

        _brandRepositoryMock.Setup(repo => repo.IsNameExists(request.Name))
           .ReturnsAsync(false);

        _supabaseServiceMock.Setup(x => x.GetPublicUrl("Brands", It.IsAny<string>()))
             .Returns("https://example.com/brand.png");

        var command = new CreateBrandCommand(request);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _supabaseServiceMock.Verify(
              x => x.UploadAsync(request.Image!, "Brands",
                 It.Is<string>(x => x == $"Brand-{result.Value!.Id}{Path.GetExtension(request.Image!.FileName)}")),
              Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);

        _brandRepositoryMock.Verify(x => x.Add(It.Is<Brand>(
            b => b.Name == request.Name &&
            b.Id == result.Value!.Id &&
            b.Description == request.Description)), Times.Once);

        result.Value.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        result.Value?.Name.Should().Be(request.Name);
        result.Value?.Description.Should().Be(request.Description);
        result.Value?.Image.Should().NotBeNull();
    }
}

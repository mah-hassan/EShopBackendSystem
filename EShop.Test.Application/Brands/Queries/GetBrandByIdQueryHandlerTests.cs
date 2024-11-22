using EShop.Application.Abstractions.Mappers;
using EShop.Application.Abstractions.Services;
using EShop.Application.Brands.Queries.GetBrandById;
using EShop.Contracts.Brand;
using EShop.Domain.Brands;
using EShop.Domain.Shared.Errors;
using FluentAssertions;
using Moq;

namespace EShop.Test.Application.Brands.Queries;

public sealed class GetBrandByIdQueryHandlerTests
{
    private readonly Mock<IBrandRepository> _brandRespositoryMock;
    private readonly Mock<ISupabaseService> _supervisorServiceMock;
    private readonly Mapper _mapper;
    public GetBrandByIdQueryHandlerTests()
    {
        _brandRespositoryMock = new();
        _supervisorServiceMock = new();
        _mapper = new();
    }


    [Fact]
    public async Task Handle_WhenBrandDoesNotExist_ReturnsAFailureResultWithNotFoundErrorType()
    {
        // Arrange
        _brandRespositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
           .ReturnsAsync(default(Brand));

        var query = new GetBrandByIdQuery(Guid.NewGuid());
        var handler = new GetBrandByIdQueryHandler(_brandRespositoryMock.Object,
                    _supervisorServiceMock.Object,
                    _mapper);
        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Errors?.First().Type.Should().Be(ErrorType.NotFound);
        _brandRespositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _supervisorServiceMock.Verify(x => x.GetPublicUrl(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenBrandExists_ReturnsASuccessResultWithBrandResponse()
    {
        // Arrange
        var brand = new Brand()
        {
            Name = "testing brand",
            Description = "this is a testing brand",
        };
        brand.Image = $"Brand-{brand.Id}.png";

        _brandRespositoryMock.Setup(repo => repo.GetByIdAsync(brand.Id))
           .ReturnsAsync(brand);

        _supervisorServiceMock.Setup(x =>
        x.GetPublicUrl("Brands", brand.Image))
            .Returns($"public/Brands/{brand.Image}");

        var query = new GetBrandByIdQuery(brand.Id);
        var handler = new GetBrandByIdQueryHandler(_brandRespositoryMock.Object,
            _supervisorServiceMock.Object,
            _mapper);

        // Act
        var result = await handler.Handle(query, default);

        // Assert

        result.IsSuccess.Should().BeTrue();

        result.Errors.Should().BeEmpty();

        result.Value.Should().NotBeNull();

        result.Value?.Image.Should().NotBeNull();

        result.Value.Should().BeEquivalentTo(new BrandResponse
        {
            Name = brand.Name,
            Description = brand.Description,
            Image = _supervisorServiceMock.Object.GetPublicUrl("Brands", brand.Image),
            Id = brand.Id,
        });

        _brandRespositoryMock.Verify(repo => repo.GetByIdAsync(brand.Id), Times.Once);
    }
}
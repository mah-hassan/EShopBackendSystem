using EShop.Application.Abstractions;
using EShop.Application.Abstractions.Mappers;
using EShop.Application.Categories.Commands.CreateCategory;
using EShop.Contracts.Category;
using EShop.Domain.Categories;
using EShop.Domain.Entities;
using EShop.Domain.Shared.Errors;
using FluentAssertions;
using Moq;

namespace EShop.Test.Application.Categories.Commands;

public sealed class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mapper _mapper;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new();
        _unitOfWorkMock = new();
        _mapper = new();
    }

    private CreateCategoryCommandHandler CreateHandler()
    {
        return new CreateCategoryCommandHandler(
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapper);
    }

    private static AddCategoryRequest CreateRequest(
        string name = "name",
        bool isParentCategory = true,
        Guid parentCategoryId = default,
        List<VariantRequest>? variants = null)
    {
        return new AddCategoryRequest
        {
            Name = name,
            IsParentCategory = isParentCategory,
            ParentCategoryId = parentCategoryId,
            Variants = variants ?? new List<VariantRequest>()
        };
    }

    [Fact]
    public async Task Handle_WhenCategoryExists_ReturnsFailureWithConflictError()
    {
        // Arrange
        _categoryRepositoryMock.Setup(repo => repo.IsNameExsists(It.IsAny<string>()))
            .ReturnsAsync(true);

        var requestDto = CreateRequest();
        var command = new CreateCategoryCommand(requestDto);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeEmpty();
        result.Errors.Should().NotBeEmpty();
        result.Errors?.First().Type.Should().Be(ErrorType.Conflict);

        _categoryRepositoryMock.Verify(repo => repo.Add(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenParentCategoryIsNotProvidedAndRequired_ReturnsFailureWithBadRequestError()
    {
        // Arrange
        var requestDto = CreateRequest(isParentCategory: false, parentCategoryId: Guid.Empty);
        var command = new CreateCategoryCommand(requestDto);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeEmpty();
        result.Errors.Should().NotBeEmpty();
        result.Errors?.First().Type.Should().Be(ErrorType.BadRequest);

        _categoryRepositoryMock.Verify(repo => repo.Add(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenParentCategoryDoesNotExist_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        var requestDto = CreateRequest(isParentCategory: false, parentCategoryId: Guid.NewGuid());

        _categoryRepositoryMock.Setup(repo => repo.IsNameExsists(It.IsAny<string>()))
            .ReturnsAsync(false);
        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(requestDto.ParentCategoryId))
            .ReturnsAsync(default(Category));

        var command = new CreateCategoryCommand(requestDto);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeEmpty();
        result.Errors.Should().NotBeEmpty();
        result.Errors?.First().Type.Should().Be(ErrorType.NotFound);

        _categoryRepositoryMock.Verify(repo => repo.Add(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCategoryIsValid_AddsCategoryAndReturnsSuccess()
    {
        // Arrange
        var requestDto = CreateRequest();
      
        _categoryRepositoryMock.Setup(repo => repo.IsNameExsists(It.IsAny<string>()))
            .ReturnsAsync(false);

        var command = new CreateCategoryCommand(requestDto);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Errors.Should().BeEmpty();

        _categoryRepositoryMock.Verify(repo => repo.Add(It.Is<Category>(c => c.Id == result.Value)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCategoryWithVariantsIsValid_AddsCategoryWithVariantsAndReturnsSuccess()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var requestDto = CreateRequest(variants: new List<VariantRequest>
        {
            new VariantRequest
            {
                Name = "Color",
                Values = new () { "red", "green", "blue" }
            }
        }, isParentCategory: false, parentCategoryId: Guid.NewGuid());
       
        _categoryRepositoryMock.Setup(repo => repo.IsNameExsists(It.IsAny<string>()))
            .ReturnsAsync(false);       
        
        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(requestDto.ParentCategoryId))
            .ReturnsAsync(new Category() { Id = requestDto.ParentCategoryId, Name = "parent"});


        var command = new CreateCategoryCommand(requestDto);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
  
        result.Errors.Should().BeEmpty();

        _categoryRepositoryMock.Verify(repo => 
        repo.Add(It.Is<Category>(c => c.Id == result.Value && 
        c.Variants.Count == requestDto.Variants.Count &&
        c.Variants.TrueForAll(
            v =>
            requestDto.Variants.Any(vr =>
                vr.Name == v.Name &&
                v.Options.Count == vr.Values.Count)))),
        Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
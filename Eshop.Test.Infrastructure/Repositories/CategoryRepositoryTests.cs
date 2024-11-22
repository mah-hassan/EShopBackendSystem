using EShop.Infrastructure.Repositories;
using Eshop.Test.Infrastructure.Data;
using EShop.Domain.Entities;
using EShop.Domain.Products;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using EShop.Test.SharedUtilities.Brands;

namespace Eshop.Test.Infrastructure.Repositories;

public sealed class CategoryRepositoryTests
{
    private readonly TestingDbContext _dbContext;
    private readonly CategoryRepository _sut;

    public CategoryRepositoryTests()
    {
        // Arrange
        _dbContext = new TestingDbContext();
        _dbContext.InitializeAsync().Wait();
        _sut = new CategoryRepository(_dbContext);

        // Seed some initial test data
        var parentCategory = new Category { Name = "ParentCategory", IsParentCategory = true };
        var subCategory = new Category { Name = "ChildCategory", IsParentCategory = false, ParentCategoryId = parentCategory.Id};

        subCategory.Variants = new List<Variant> { new Variant { Name = "Size" } };
        subCategory.Brands = BrandFaker.CreateList(3).ToHashSet();
        _dbContext.Categories.Add(parentCategory);
        _dbContext.Categories.Add(subCategory);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task IsNameExists_ShouldReturnTrue_WhenCategoryWithNameExists()
    {
        // Act
        var result = await _sut.IsNameExsists("ParentCategory");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsNameExists_ShouldReturnFalse_WhenCategoryWithNameDoesNotExist()
    {
        // Act
        var result = await _sut.IsNameExsists("NonExistentCategory");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategoryWithoutVariantsAndBrands_WhenCategoryIsParent()
    {
        // Arrange
        var parentCategory = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Name == "ParentCategory");

        // Act
        var result = await _sut.GetByIdAsync(parentCategory!.Id);

        // Assert
        result.Should().NotBeNull();
        result?.Variants.Should().BeEmpty();
        result?.Brands.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategoryWithVariantsAndBrands_WhenCategoryIsNotParent()
    {
        // Arrange
        var childCategory = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Name == "ChildCategory");

        // Act
        var result = await _sut.GetByIdAsync(childCategory!.Id);

        // Assert
        result.Should().NotBeNull();
        result?.Variants.Should().NotBeEmpty();
        result?.Brands.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
}

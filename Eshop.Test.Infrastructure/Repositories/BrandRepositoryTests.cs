using Eshop.Test.Infrastructure.Data;
using EShop.Domain.Brands;
using EShop.Domain.Entities;
using EShop.Infrastructure.Repositories;
using EShop.Test.SharedUtilities.Brands;
using EShop.Test.SharedUtilities.Products;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Eshop.Test.Infrastructure.Repositories;

public sealed class BrandRepositoryTests
{
    private readonly TestingDbContext _dbContext = new();
    private Brand brand;
    public BrandRepositoryTests()
    {

        _dbContext.InitializeAsync().GetAwaiter().GetResult();

        brand = BrandFaker.Create();
        _dbContext.Brands.Add(brand);

        var category = new Category() { Name = "Category" };
        _dbContext.Categories.Add(category);

        _dbContext.SaveChanges();

        var product = ProductFaker.CreateTestProduct(includeVariants: false);
        product.CategoryId = category.Id;
        product.BrandId = brand.Id;
        _dbContext.Products.Add(product);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Add_ShouldAddBrandToBrandsDbSet()
    {
        // Arrange
        var brand = BrandFaker.Create();
        var sut = new BrandRepository(_dbContext);
        // Act
        sut.Add(brand);
        await _dbContext.SaveChangesAsync();
        // Assert
        _dbContext.Brands.Should().Contain(brand);
    }


    [Fact]
    public Task Add_ShouldThrowException_WhenAddingBrandWithEnxistingName()
    {
        // Arrange
        var bnewBand = BrandFaker.Create();
        bnewBand.Name = brand.Name;
        var sut = new BrandRepository(_dbContext);
        // Act
        sut.Add(bnewBand);
        var func =  () => _dbContext.SaveChanges();
        // Assert
        func.Should().Throw<DbUpdateException>().WithInnerException<SqliteException>();
        return Task.CompletedTask; 
    }


    [Fact]
    public async Task GetByIdIncludingProductsAsync_ShouldReturnBrandWithProducts()
    {
        // Arrange
        var sut = new BrandRepository(_dbContext);

        // Act
        var result = await sut.GetByIdIncludingProductsAsync(brand.Id);

        // Assert
        result.Should().Be(brand);
        result!.Products.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Update_ShouldUpdateBrandInBrandsDbSet()
    {
        // Arrange
        var sut = new BrandRepository(_dbContext);
        var updatedBrand = await sut.GetByIdAsync(brand.Id);

        // Act
        updatedBrand!.Name = "updated";
        sut.Update(updatedBrand!);
        var effectedRows = await _dbContext.SaveChangesAsync();
        // Assert
        effectedRows.Should().Be(1);
    }

    [Fact]
    public async Task Delete_ShouldRemoveBrandFromBrandsDbSet()
    {
        // Arrange
        var sut = new BrandRepository(_dbContext);
        // Act
        sut.Delete(brand);
        var effectedRows = await _dbContext.SaveChangesAsync();
        // Assert
        effectedRows.Should().BeGreaterThan(0);
        _dbContext.Brands.Should().NotContain(brand);
    }

    [Fact]
    public async Task IsNameExists_ShouldReturnTrue_WhenNameExists()
    {
        // Arrange
        var sut = new BrandRepository(_dbContext);

        // Act
        var result = await sut.IsNameExists(brand.Name);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsNameExists_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        // Arrange
        var sut = new BrandRepository(_dbContext);
        var uniqueName = "Unique Brand Name";

        // Act
        var result = await sut.IsNameExists(uniqueName);

        // Assert
        result.Should().BeFalse();
    }

}
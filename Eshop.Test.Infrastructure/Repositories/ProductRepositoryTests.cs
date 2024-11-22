using EShop.Application.Products.Queries.GetProducts;
using EShop.Infrastructure.Repositories;
using Eshop.Test.Infrastructure.Data;
using EShop.Test.SharedUtilities.Products;
using FluentAssertions;
using EShop.Test.SharedUtilities.Brands;
using EShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EShop.Domain.Brands;
using EShop.Domain.Products;

namespace Eshop.Test.Infrastructure.Repositories;

public sealed class ProductRepositoryTests
{
    private readonly TestingDbContext _dbContext;
    private readonly ProductRepository _sut;

    public ProductRepositoryTests()
    {
        _dbContext = new TestingDbContext();
        _dbContext.InitializeAsync().Wait();
        _sut = new ProductRepository(_dbContext);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var product = ProductFaker.CreateTestProduct(includeVariants: false);
        var brand = new Brand() { Name = "x brand" };
        _dbContext.Brands.Add(brand);
        var category = new Category()
        {
            Name = "Category",
        };
        category.Brands.Add(brand);
        _dbContext.Categories.Add(category);

        product.BrandId = brand.Id;
        product.CategoryId = category.Id;

        _dbContext.Products.Add(product);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCountAndNotApplayPagenation_WhenSpecificationIsApplied()
    {
        // Arrange
        var brand = BrandFaker.Create();
        _dbContext.Brands.Add(brand);
        var categoryId = await _dbContext.Categories.Select(c => c.Id).FirstAsync();
        var testProduct = ProductFaker.CreateTestProduct(brandId: brand.Id, categoryId: categoryId, includeVariants: false);
        _dbContext.Products.Add(testProduct);
        await _dbContext.SaveChangesAsync();

        var spec = new GetProductsSecification(new GetProductsQuery
        {
            brandId = brand.Id,
        });

        // Act
        var result = await _sut.CountAsync(spec);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenIdExists()
    {
        // Arrange
        var brandId = await _dbContext.Brands.Select(b => b.Id).FirstAsync();
        var categoryId = await _dbContext.Categories.Select(c => c.Id).FirstAsync();

        var testProduct = ProductFaker.CreateTestProduct(brandId: brandId, categoryId: categoryId, includeVariants: false);
        _dbContext.Products.Add(testProduct);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(testProduct.Id);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductsWithSpecificationAsync_ShouldReturnFilteredProducts()
    {
        // Arrange
        var categoryId = await _dbContext.Categories.Select(c => c.Id).FirstAsync();

        var spec = new GetProductsSecification(new GetProductsQuery
        {
            categoryId = categoryId
        });

        // Act
        var result = await _sut.GetProductsWithSpecificationAsync(spec, default);

        // Assert
        result.Should().ContainSingle();
        result.First().CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task IsSkuUnique_ShouldReturnFalse_WhenSkuAlreadyExists()
    {
        // Arrange
        var sku = await _dbContext.Products.Select(p => p.Sku).FirstOrDefaultAsync();
        // Act
        var result = await _sut.IsSkuUnique(sku);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSkuUnique_ShouldReturnTrue_WhenSkuDoesNotExist()
    {
        // Act
        var result = await _sut.IsSkuUnique("UniqueSku123");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ShouldDeleteProductAttribuates_WhenProductIsDeletedSuccessfuly()
    {

        // Arrange  

        var brandId = await _dbContext.Brands.Select(b => b.Id).FirstAsync();
        var categoryId = await _dbContext.Categories.Select(c => c.Id).FirstAsync();
        var testProduct = ProductFaker.CreateTestProduct(brandId: brandId, categoryId: categoryId, includeVariants: true);
        var variants = testProduct.Variants.Select(x => x.Key).Distinct().ToList();
        variants.ForEach(v => v.CategoryId = categoryId);
        _dbContext.Set<Variant>().AddRange(variants);
        _dbContext.Set<VariantOption>().AddRange(testProduct.VariantOptions);
        _dbContext.Products.Add(testProduct);
        _dbContext.SaveChanges();

        // Act
        _sut.Delete(testProduct);
        await _dbContext.SaveChangesAsync();

        // Assert
        _dbContext.Products.Find(testProduct.Id).Should().BeNull();
        _dbContext.ProductAttributes.Where(pa => pa.ProductId == testProduct.Id).Count().Should().Be(0);

    }
}
using EShop.Domain.Brands;
using EShop.Domain.Orders;
using EShop.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.Data;

public class ApplicationDbContext
    : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }
    protected ApplicationDbContext()
    {
        
    }
    public DbSet<Product> Products { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<CategoryBrands> CategoryBrands { get; set; }
    public DbSet<ProductAttribuates> ProductAttributes { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

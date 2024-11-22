using EShop.Application.Abstractions;
using EShop.Application.Abstractions.Services;
using EShop.Domain.Brands;
using EShop.Domain.Categories;
using EShop.Domain.Invoices;
using EShop.Domain.Orders;
using EShop.Domain.Products;
using EShop.Domain.ShoppingCarts;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Repositories;
using EShop.Infrastructure.SeedingData;
using EShop.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Supabase;

namespace EShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? redisConnection = configuration.GetConnectionString("Redis") ?? throw new NullReferenceException("redis connection is null");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("OnlineEShopDb"));
        });


        services.AddStackExchangeRedisCache(config =>
        {
            config.Configuration = redisConnection;
        });

        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var configuration = ConfigurationOptions.Parse(redisConnection);
            configuration.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configuration);
        });


        services.AddFluentEmail(configuration["Email:From"], configuration["Email:Sender"])
            .AddSmtpSender(configuration["Email:Host"],             
                configuration.GetValue<int>("Email:Port"),
                configuration["Email:from"],
                configuration["Email:Password"])         
            .AddRazorRenderer();

        services.AddScoped(_ => new Supabase.Client
        (
             configuration["Supabase:Url"] ?? throw new NullReferenceException("Supabase URL is required"),
             configuration["Supabase:Key"],
             new SupabaseOptions
             {
                 AutoRefreshToken = true,
                 AutoConnectRealtime = true,
             }
        ));
        services.AddScoped<IEmailService, EmailService>();  
        services.AddScoped<ICachService, CachService>();
        services.AddScoped<ISupabaseService, SupabaseService>();
        services.AddScoped<IElasticSearchService, ElasticSearchService>();
        services.AddScoped<IEventBus, EventBus>();
        services.AddScoped<IStripeService, StripeService>();

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IProductAttribuatesRepository, ProductAttribuatesRepository>();
        services.AddScoped<ICategoryBrandsRepository, CategoryBrandsRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IDeliveryMethodRepository, DeliveryMethodRepository>();

        return services;
    }
    public static async Task SeedDataAsync(this IServiceCollection services)
    {
        using var sp = services.BuildServiceProvider();
        var dbContext = sp.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        await DelieveryMethodSeeding.SeedAsync(dbContext);
    }
}
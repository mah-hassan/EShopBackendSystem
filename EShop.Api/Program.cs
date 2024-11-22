
using EShop.Api.Authentication;
using EShop.Application;
using EShop.Domain.Settings;
using EShop.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Host.UseSerilog((context, loggerConfigurations) =>
    loggerConfigurations.ReadFrom.Configuration(context.Configuration));

// Configure Complex Configuration Options
builder.Services.Configure<StripeSettings>
    (configuration.GetSection(nameof(StripeSettings)));

builder.Services.Configure<MessageBrockerSettings>
    (configuration.GetSection(MessageBrockerSettings.ConfigurationSection));
builder.Services.Configure<ElasticSearchSettings>
    (configuration.GetSection(ElasticSearchSettings.ConfigurationSection));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.ResolveConflictingActions(x => x.Last());
});
builder.Services.AddAntiforgery();
builder.Services.AddOutputCache();

builder.Services.AddInfrastructure(configuration)
    .AddApplication(configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(policy =>
{
    
    policy.AddPolicy("eshop",c =>
    {
        c.AllowAnyOrigin();
        c.AllowAnyHeader();
        c.AllowAnyMethod();
       
    });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = configuration["Jwt:Authority"];  
    options.Audience = configuration["Jwt:Audience"];              
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = configuration["Jwt:Issuer"],  
        ValidateAudience = true,
        ValidAudience = configuration["Jwt:Audience"],              
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.Admin, policy =>
        policy.Requirements.Add(new KeycloakRoleRequirement("eshop-backend-client", Policies.Admin)));
});

builder.Services.AddSingleton<IAuthorizationHandler, KeycloakRoleHandler>();


builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpoints();

builder.Services.AddMessageBrocker();

await builder.Services.SeedDataAsync();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.MapGet("/payment-success", () => "Payment Successded");

app.MapGet("/anft", (IAntiforgery antiforgery,HttpContext context) =>
{
    var token = antiforgery.GetAndStoreTokens(context);
    return Results.Ok(token);
}).RequireAuthorization();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("eshop");
app.UseAntiforgery();

app.MapGet("/", () => "Hellow From Server")
    .RequireAuthorization();

app.MapEndpoints();

app.UseOutputCache();
app.Run();
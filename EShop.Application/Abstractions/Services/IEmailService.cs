namespace EShop.Application.Abstractions.Services;

public interface IEmailService
{
    Task SendAsync<T>(string to, string subject, string templete, T model);
}
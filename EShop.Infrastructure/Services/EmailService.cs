using EShop.Application.Abstractions.Services;
using FluentEmail.Core;

namespace EShop.Infrastructure.Services;

internal sealed class EmailService(IFluentEmail fluentEmail)
    : IEmailService
{
    public async Task SendAsync<T>(string to, string subject, string templete, T model)
    {

        string path = Path.Combine(Directory.GetCurrentDirectory(),
            "wwwroot",
            "Templetes",
            templete);
        var message = await File.ReadAllTextAsync(path);

        await fluentEmail
            .To(to)
            .Subject(subject)
            .UsingTemplateFromFile(path, model, isHtml: true)
            .SendAsync();
    }
}
using Microsoft.AspNetCore.Http;

namespace EShop.Application.Abstractions.Services;

public interface ISupabaseService
{
    string GetPublicUrl(string backet, string supabasePath);
    Task<string> UploadAsync(IFormFile file, string backet, string? supabasePath = default);
    Task DeleteFileAsync(string backet, string supabasePath);
}
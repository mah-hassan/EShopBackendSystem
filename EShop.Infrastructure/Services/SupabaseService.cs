using EShop.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;

namespace EShop.Infrastructure.Services;

internal sealed class SupabaseService : ISupabaseService
{
    private readonly Supabase.Client _supabaseClient;

    public SupabaseService(Supabase.Client client)
    {
        _supabaseClient = client;
    }

    public async Task DeleteFileAsync(string backet, string supabasePath)
    {
        await _supabaseClient.Storage.From(backet).Remove(supabasePath);
    }

    public string GetPublicUrl(string backet, string supabasePath)
        => _supabaseClient.Storage.From(backet).GetPublicUrl(supabasePath);

   

   

    public async Task<string> UploadAsync(IFormFile file, string backet, string? supabasePath = null)
    {
        using MemoryStream stream = new();
        await file.CopyToAsync(stream);
        supabasePath = supabasePath ?? GenerateRondomSupabasePath(file.FileName, backet);
        await _supabaseClient.Storage.From(backet)
            .Upload(stream.ToArray(), supabasePath);
        return supabasePath;    
    }
    private string GenerateRondomSupabasePath(string fileName, string backet)
    {
        return $"{backet}-{Path.GetRandomFileName() + Path.GetExtension(fileName)}";
    }

}
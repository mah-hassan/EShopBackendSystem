using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace EShop.Test.SharedUtilities;

public static class ImageGeneratorUtility
{
    public static IFormFile CreateFormFile(string content, string fileName, string contentType = "text/plain")
    {
        var memoryStream = new MemoryStream();
        var writer = new StreamWriter(memoryStream);
        writer.Write(content);
        writer.Flush();
        memoryStream.Position = 0;

        var formFile = new FormFile(memoryStream, 0, memoryStream.Length, "id_from_form", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
            ContentDisposition = $"inline; filename={fileName}"
        };

        return formFile;
    }
}
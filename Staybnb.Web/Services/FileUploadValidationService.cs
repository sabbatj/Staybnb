using Microsoft.AspNetCore.Http;

namespace Staybnb.Web.Services;

public static class FileUploadValidationService
{
    private const long MaxImageSize = 5 * 1024 * 1024;      // 5 MB
    private const long MaxDocumentSize = 10 * 1024 * 1024; // 10 MB

    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

    private static readonly HashSet<string> DocumentExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png"
        };

    public static bool IsValidImage(IFormFile file)
    {
        if (file == null || file.Length <= 0 || file.Length > MaxImageSize)
            return false;

        var extension = Path.GetExtension(file.FileName);

        return ImageExtensions.Contains(extension);
    }

    public static bool IsValidDocument(IFormFile file)
    {
        if (file == null || file.Length <= 0 || file.Length > MaxDocumentSize)
            return false;

        var extension = Path.GetExtension(file.FileName);

        return DocumentExtensions.Contains(extension);
    }
}

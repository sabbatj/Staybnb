using Microsoft.AspNetCore.Http;
using Staybnb.Web.Services;
using Xunit;

namespace Staybnb.Web.Tests;

public class FileUploadValidationTests
{
    private static IFormFile CreateFile(
        string fileName,
        long length)
    {
        var stream = new MemoryStream(new byte[Math.Min((int)length, 1024)]);

        return new FormFile(stream, 0, length, "file", fileName);
    }

    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("photo.jpeg")]
    [InlineData("photo.png")]
    [InlineData("photo.webp")]
    public void Accepts_Valid_Property_Image_Extensions(string fileName)
    {
        var file = CreateFile(fileName, 1024);

        Assert.True(FileUploadValidationService.IsValidImage(file));
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("photo.gif")]
    [InlineData("photo.svg")]
    [InlineData("photo.exe")]
    public void Rejects_Invalid_Property_Image_Extensions(string fileName)
    {
        var file = CreateFile(fileName, 1024);

        Assert.False(FileUploadValidationService.IsValidImage(file));
    }

    [Fact]
    public void Accepts_Pdf_As_Guest_Document()
    {
        var file = CreateFile("passport.pdf", 1024);

        Assert.True(FileUploadValidationService.IsValidDocument(file));
    }

    [Fact]
    public void Rejects_Empty_Document()
    {
        var file = CreateFile("passport.pdf", 0);

        Assert.False(FileUploadValidationService.IsValidDocument(file));
    }

    [Fact]
    public void Rejects_Image_Larger_Than_5MB()
    {
        var file = CreateFile("large.jpg", 5 * 1024 * 1024 + 1);

        Assert.False(FileUploadValidationService.IsValidImage(file));
    }

    [Fact]
    public void Rejects_Document_Larger_Than_10MB()
    {
        var file = CreateFile("large.pdf", 10 * 1024 * 1024 + 1);

        Assert.False(FileUploadValidationService.IsValidDocument(file));
    }
}

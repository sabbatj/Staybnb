using Staybnb.Web.Models;
using Xunit;

namespace Staybnb.Web.Tests;

public class Requirement2HostingTests
{
    [Fact]
    public void PropertyImage_StoresImageForProperty()
    {
        var image = new PropertyImage
        {
            PropertyId = 10,
            ImageUrl = "property.jpg"
        };

        Assert.Equal(10, image.PropertyId);
        Assert.Equal("property.jpg", image.ImageUrl);
    }

    [Fact]
    public void CheckInProcess_StoresRulesAndRequiredDocuments()
    {
        var process = new CheckInProcess
        {
            PropertyId = 10,
            Title = "Standard Check-in",
            StepsJson = "[\"Verify ID\",\"Collect keys\"]",
            RequiredDocumentsJson = "[\"ID\",\"Passport\"]"
        };

        Assert.Equal(10, process.PropertyId);
        Assert.Contains("Verify ID", process.StepsJson);
        Assert.Contains("ID", process.RequiredDocumentsJson);
        Assert.Contains("Passport", process.RequiredDocumentsJson);
    }
}

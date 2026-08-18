using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Xunit;

namespace Staybnb.Tests;

public class CheckInProcessTests
{
    private static async Task<(ApplicationDbContext context, HostProperty property)> SeedPropertyAsync(string dbName)
    {
        var provider = TestServiceProviderFactory.Create(dbName);
        var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var property = new HostProperty
        {
            Title = "Test Cabin",
            Description = "Property for check-in testing",
            PricePerNight = 1000m,
            HostId = "host-1",
            Address = "1 Test Street",
            CleaningFee = 150m,
            ServiceFee = 100m,
            IsActive = true
        };

        context.HostProperties.Add(property);
        await context.SaveChangesAsync();

        return (context, property);
    }

    [Fact]
    public async Task CheckInProcess_CanBeCreatedAndPersisted()
    {
        var (context, property) =
            await SeedPropertyAsync(nameof(CheckInProcess_CanBeCreatedAndPersisted));

        var process = new CheckInProcess
        {
            PropertyId = property.Id,
            Title = "Guest Check-in Instructions",
            StepsJson = """["Bring your ID","Arrive after 2 PM","Contact host on arrival"]""",
            RequiredDocumentsJson = """["ID"]"""
        };

        context.CheckInProcesses.Add(process);
        await context.SaveChangesAsync();

        var saved = await context.CheckInProcesses
            .FirstOrDefaultAsync(c => c.PropertyId == property.Id);

        Assert.NotNull(saved);
        Assert.Equal("Guest Check-in Instructions", saved!.Title);
        Assert.Equal(
            """["Bring your ID","Arrive after 2 PM","Contact host on arrival"]""",
            saved.StepsJson);
        Assert.Equal("""["ID"]""", saved.RequiredDocumentsJson);
    }

    [Fact]
    public async Task CheckInProcess_CanRequireIdAndPassport()
    {
        var (context, property) =
            await SeedPropertyAsync(nameof(CheckInProcess_CanRequireIdAndPassport));

        var process = new CheckInProcess
        {
            PropertyId = property.Id,
            Title = "Required Documents",
            StepsJson = """["Complete check-in"]""",
            RequiredDocumentsJson = """["ID","Passport"]"""
        };

        context.CheckInProcesses.Add(process);
        await context.SaveChangesAsync();

        var saved = await context.CheckInProcesses
            .SingleAsync(c => c.PropertyId == property.Id);

        Assert.Contains("ID", saved.RequiredDocumentsJson);
        Assert.Contains("Passport", saved.RequiredDocumentsJson);
    }

    [Fact]
    public async Task CheckInProcess_CanBeUpdated()
    {
        var (context, property) =
            await SeedPropertyAsync(nameof(CheckInProcess_CanBeUpdated));

        var process = new CheckInProcess
        {
            PropertyId = property.Id,
            Title = "Initial Instructions",
            StepsJson = """["Initial step"]""",
            RequiredDocumentsJson = """["ID"]"""
        };

        context.CheckInProcesses.Add(process);
        await context.SaveChangesAsync();

        process.Title = "Updated Instructions";
        process.StepsJson = """["Bring ID","Arrive at 14:00"]""";
        process.RequiredDocumentsJson = """["Passport"]""";

        await context.SaveChangesAsync();

        var updated = await context.CheckInProcesses
            .AsNoTracking()
            .SingleAsync(c => c.PropertyId == property.Id);

        Assert.Equal("Updated Instructions", updated.Title);
        Assert.Equal(
            """["Bring ID","Arrive at 14:00"]""",
            updated.StepsJson);
        Assert.Equal("""["Passport"]""", updated.RequiredDocumentsJson);
    }

    [Fact]
    public async Task CheckInProcess_IsLinkedToCorrectProperty()
    {
        var provider = TestServiceProviderFactory.Create(
            nameof(CheckInProcess_IsLinkedToCorrectProperty));

        var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var property1 = new HostProperty
        {
            Title = "Property One",
            Description = "First property",
            PricePerNight = 1000m,
            HostId = "host-1",
            Address = "1 Test Street",
            CleaningFee = 100m,
            ServiceFee = 50m,
            IsActive = true
        };

        var property2 = new HostProperty
        {
            Title = "Property Two",
            Description = "Second property",
            PricePerNight = 1500m,
            HostId = "host-2",
            Address = "2 Test Street",
            CleaningFee = 100m,
            ServiceFee = 50m,
            IsActive = true
        };

        context.HostProperties.AddRange(property1, property2);
        await context.SaveChangesAsync();

        context.CheckInProcesses.Add(new CheckInProcess
        {
            PropertyId = property1.Id,
            Title = "Property One Check-in",
            StepsJson = """["Step 1"]""",
            RequiredDocumentsJson = """["ID"]"""
        });

        await context.SaveChangesAsync();

        var propertyOneProcess = await context.CheckInProcesses
            .SingleOrDefaultAsync(c => c.PropertyId == property1.Id);

        var propertyTwoProcess = await context.CheckInProcesses
            .SingleOrDefaultAsync(c => c.PropertyId == property2.Id);

        Assert.NotNull(propertyOneProcess);
        Assert.Null(propertyTwoProcess);
        Assert.Equal(property1.Id, propertyOneProcess!.PropertyId);
    }

    [Fact]
    public async Task CheckInProcess_RequiredDocumentsJson_CanBeDeserialized()
    {
        var (context, property) =
            await SeedPropertyAsync(nameof(CheckInProcess_RequiredDocumentsJson_CanBeDeserialized));

        var process = new CheckInProcess
        {
            PropertyId = property.Id,
            Title = "Document Requirements",
            StepsJson = """["Complete check-in"]""",
            RequiredDocumentsJson = """["ID","Passport"]"""
        };

        context.CheckInProcesses.Add(process);
        await context.SaveChangesAsync();

        var saved = await context.CheckInProcesses
            .AsNoTracking()
            .SingleAsync(c => c.PropertyId == property.Id);

        var documents =
            System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                saved.RequiredDocumentsJson);

        Assert.NotNull(documents);
        Assert.Contains("ID", documents!);
        Assert.Contains("Passport", documents!);
        Assert.Equal(2, documents.Count);
    }

    [Fact]
    public async Task CheckInProcess_CanHaveNoRequiredDocuments()
    {
        var (context, property) =
            await SeedPropertyAsync(nameof(CheckInProcess_CanHaveNoRequiredDocuments));

        var process = new CheckInProcess
        {
            PropertyId = property.Id,
            Title = "No Documents Required",
            StepsJson = """["Follow the instructions"]""",
            RequiredDocumentsJson = "[]"
        };

        context.CheckInProcesses.Add(process);
        await context.SaveChangesAsync();

        var saved = await context.CheckInProcesses
            .AsNoTracking()
            .SingleAsync(c => c.PropertyId == property.Id);

        var documents =
            System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                saved.RequiredDocumentsJson);

        Assert.NotNull(documents);
        Assert.Empty(documents!);
    }

    [Fact]
    public async Task CheckInProcess_CanLoadThroughPropertyRelationship()
    {
        var (context, property) =
            await SeedPropertyAsync(nameof(CheckInProcess_CanLoadThroughPropertyRelationship));

        context.CheckInProcesses.Add(new CheckInProcess
        {
            PropertyId = property.Id,
            Title = "Property Check-in",
            StepsJson = """["Arrive at 14:00"]""",
            RequiredDocumentsJson = """["ID"]"""
        });

        await context.SaveChangesAsync();

        var loadedProperty = await context.HostProperties
            .Include(p => p.CheckInProcess)
            .SingleAsync(p => p.Id == property.Id);

        Assert.NotNull(loadedProperty.CheckInProcess);
        Assert.Equal(
            "Property Check-in",
            loadedProperty.CheckInProcess!.Title);
        Assert.Equal(
            """["ID"]""",
            loadedProperty.CheckInProcess.RequiredDocumentsJson);
    }
}

namespace Staybnb.Web.Models;

public class CheckInProcess
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public HostProperty? Property { get; set; }

    public string Title { get; set; } = string.Empty;
    public string StepsJson { get; set; } = string.Empty;

    // JSON array of required guest documents, e.g. ["ID", "Passport"]
    public string RequiredDocumentsJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

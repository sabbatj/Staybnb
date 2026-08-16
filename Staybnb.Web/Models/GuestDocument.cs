namespace Staybnb.Web.Models;

public class GuestDocument
{
    public int Id { get; set; }
    public int GuestCheckInId { get; set; }
    public GuestCheckIn? GuestCheckIn { get; set; }

    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
}

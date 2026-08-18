namespace Staybnb.Web.Models.ViewModels;

public class HostManagementRow
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int PropertyCount { get; set; }
    public int ActivePropertyCount { get; set; }
}

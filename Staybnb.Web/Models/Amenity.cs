namespace Staybnb.Web.Models;

public class Amenity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;

    public ICollection<HostProperty> Properties { get; set; } = new List<HostProperty>();
}

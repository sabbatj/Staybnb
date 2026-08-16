using System.ComponentModel.DataAnnotations;

namespace Staybnb.Web.Models.ViewModels;

public class MessageComposeViewModel
{
    [Required]
    public string ReceiverId { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace MacroMail.Models.Dto;

public class ContactRequest
{
    public string Uid { get; set; }

    [Required] public string Email { get; set; }

    public string Name { get; set; }
}
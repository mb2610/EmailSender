using System.ComponentModel.DataAnnotations;

namespace MacroMail.Models.Dto;

public class SendingRequest
{
    [Required] public Guid                        Sender  { get; set; }
    [Required] public string                      Reply   { get; set; }
    [Required] public string                      Subject { get; set; }
    [Required] public string                      Content { get; set; }
    [Required] public ContactRequest              To      { get; set; }
    [Required] public IEnumerable<ContactRequest> Ccs     { get; set; }
    public            IDictionary<string, object> Data    { get; set; }
    
    public int Priority { get; set; }
}
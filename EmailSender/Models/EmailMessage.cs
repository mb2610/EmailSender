namespace MacroMail.Models;

public class EmailMessageServerInformation
{
    public Guid     Uid        { get; set; }
    public Guid     Sender     { get; set; }
    public string   IpAddress  { get; set; }
    public DateTime SentAtDate { get; set; }
}

public class EmailMessage
{
    public Guid     Uid     { get; set; }
    public Guid     Sender  { get; set; }
    public string   To      { get; set; }
    public string[] Ccs     { get; set; }
    public string   Subject { get; set; }
    public string   Content { get; set; }
    public string   Reply   { get; set; }
}
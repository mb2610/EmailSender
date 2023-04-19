namespace MacroMail.Models.Dao;

public class EmailSenderDao
{
    public Guid     Uid             { get; set; }
    public string   Name            { get; set; }
    public string   Email           { get; set; }
    public string   Password        { get; set; }
    public string   Host            { get; set; }
    public int      Port            { get; set; }
    public bool     Enable          { get; set; }
    public string[] AllowedIpSender { get; set; }

    public bool             IsGroupSending { get; set; }
    public EmailSenderDao[] SubGroups      { get; set; }
}
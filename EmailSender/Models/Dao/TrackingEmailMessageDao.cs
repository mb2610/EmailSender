namespace MacroMail.Models.Dao;

public class TrackingEmailMessageDao
{
    public Guid                 Uid      { get; set; }
    public Guid                 GroupUid { get; set; }
    public GroupEmailMessageDao Group    { get; set; }

    public string ExternalUid { get; set; }
    public string ExternalCCs { get; set; }

    public string    SentFromIp { get; set; }
    public DateTime? SentDate   { get; set; }

    public byte[] MimeMessage { get; set; }

    public DateTime? LastOpenedDate { get; set; }

    public bool   IsError      { get; set; } = false;
    public string ErrorMessage { get; set; }
}
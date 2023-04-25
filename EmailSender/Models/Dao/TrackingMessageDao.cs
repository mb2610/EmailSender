namespace MacroMail.Models.Dao;

public class TrackingMessageDao
{
    public Guid                 Uid      { get; set; }
    public Guid                 GroupUid { get; set; }
    public GroupMessageDao Group    { get; set; }

    public string    ExternalUid { get; set; }
    public string[]? ExternalCCs { get; set; }
    public byte[]    MimeMessage { get; set; }

    public string    SentHost { get; set; }
    public DateTime? SentDate { get; set; }

    public DateTime? LastOpenedDate { get; set; }
    public int?      NumberOpening  { get; set; }

    public bool    IsErrorSending { get; set; } = false;
    public string? ErrorMessage   { get; set; }
}
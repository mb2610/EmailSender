namespace MacroMail.Models.Dao;

public class PendingMessageDao
{
    public Guid            Uid      { get; set; }
    public Guid            GroupUid { get; set; }
    public GroupMessageDao Group    { get; set; }

    public string    ExternalUid { get; set; }
    public string[]? ExternalCCs { get; set; }
    public byte[]    MimeMessage { get; set; }
}
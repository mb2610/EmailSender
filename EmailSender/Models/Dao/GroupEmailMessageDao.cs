namespace MacroMail.Models.Dao;

public class GroupEmailMessageDao
{
    public Guid   Uid         { get; set; }
    public Guid   ExternalUid { get; set; }
    public string Reply       { get; set; }
    public string Subject     { get; set; }
    public string Content     { get; set; }
    public long   Counter     { get; set; }

    public DateTime  ReceiveDate { get; set; }
    public DateTime? StartDate   { get; set; }
    public DateTime? FinishDate  { get; set; }
}
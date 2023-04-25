using MacroMail.Models.Dao;
using Microsoft.EntityFrameworkCore;

namespace MacroMail.DbAccess;

public class DataContext : DbContext
{
    public DbSet<GroupMessageDao>    GroupEmails     { get; set; }
    public DbSet<TrackingMessageDao> TrackingEmails  { get; set; }
    public DbSet<PendingMessageDao>  PendingEmails   { get; set; }
    public DbSet<EmailSenderDao>          EmailSenderDaos { get; set; }
}
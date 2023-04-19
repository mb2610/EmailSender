using MacroMail.Models.Dao;
using Microsoft.EntityFrameworkCore;

namespace MacroMail.DbAccess;

public class DataContext : DbContext
{
    public DbSet<GroupEmailMessageDao>    GroupEmails     { get; set; }
    public DbSet<TrackingEmailMessageDao> TrackingEmails  { get; set; }
    public DbSet<EmailSenderDao>          EmailSenderDaos { get; set; }
}
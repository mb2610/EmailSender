namespace MacroMail.Models.Configuration;

public class QuotaConfiguration
{
    public long     MaxQuota { get; set; }
    public TimeSpan Period   { get; set; }
}
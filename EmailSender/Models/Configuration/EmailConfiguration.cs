namespace MacroMail.Models.Configuration;

public class EmailConfiguration
{
    public Guid     Uid       { get; set; }
    public string   Server    { get; set; }
    public int      Port      { get; set; }
    public string   Email     { get; set; }
    public string   Password  { get; set; }
    public string[] IpConfigs { get; set; }
}
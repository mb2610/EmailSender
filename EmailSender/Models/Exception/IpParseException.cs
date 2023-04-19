namespace MacroMail.Models.Exception;

public class IpParseException : System.Exception
{
    public IpParseException(string ip) : base($"Ip '{ip}' not a correct format") { }
}
namespace MacroMail.Models.Exception;

public class NotFoundEmailConfigurationException : System.Exception
{
    public NotFoundEmailConfigurationException(Guid email) : base($"Mapping ip for email '{email}' not found'") { }
}
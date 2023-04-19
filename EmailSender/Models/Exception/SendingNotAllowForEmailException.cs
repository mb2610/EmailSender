namespace MacroMail.Models.Exception;

public class SendingNotAllowForEmailException : System.Exception
{
    public SendingNotAllowForEmailException(Guid email) : base($"Sending not allowed for email '{email}''") { }
}
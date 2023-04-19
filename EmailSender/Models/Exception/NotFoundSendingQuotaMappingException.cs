namespace MacroMail.Models.Exception;

public class NotFoundSendingQuotaMappingException : System.Exception
{
    public NotFoundSendingQuotaMappingException(Guid ipAddress) :
        base($"Sending quota for '{ipAddress}' not found'") { }
}
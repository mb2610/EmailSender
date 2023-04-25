namespace MacroMail.Service.Sending;

public interface IRateLimiter
{
    Task<bool> TryGetValue(string   ipAddress);
    Task<int>  AvailableRate(string ipAddress);
}
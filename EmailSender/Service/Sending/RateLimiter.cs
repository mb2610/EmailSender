using System.Collections.Concurrent;

namespace MacroMail.Service.Sending;

public class RateLimiter : IRateLimiter
{
    private const           int                               MaxRequestPerInterval = 250;
    private readonly        TimeSpan                          _interval;
    private static readonly ConcurrentDictionary<string, int> RequestCountPerIpAddress = new();

    public RateLimiter(TimeSpan interval) { _interval = interval; }

    public Task<bool> TryGetValue(string ipAddress)
    {
        if (RequestCountPerIpAddress.TryGetValue(ipAddress, out var countSending))
        {
            if (countSending >= MaxRequestPerInterval)
                return Task.FromResult(false);
            RequestCountPerIpAddress[ipAddress] = countSending + 1;
        }
        else
            RequestCountPerIpAddress.TryAdd(ipAddress, 1);

        Task.Delay(_interval).ContinueWith((t) => { RequestCountPerIpAddress.TryRemove(ipAddress, out var _); });
        return Task.FromResult(true);
    }

    public Task<int> AvailableRate(string ipAddress)
    {
        return Task.FromResult(RequestCountPerIpAddress.TryGetValue(ipAddress, out var countSending)
                                   ? countSending
                                   : MaxRequestPerInterval);
    }
}
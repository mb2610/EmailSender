using System.Collections.Concurrent;

namespace MacroMail.Service.Sending;

public class RateLimiter : IRateLimiter
{
    private const    int                               MaxRequestPerInterval = 250;
    private readonly TimeSpan                          _interval;
    private readonly ConcurrentDictionary<string, int> _requestCountPerIpAddress = new();

    public RateLimiter(TimeSpan interval) { _interval = interval; }

    public Task<bool> TryGetValue(string ipAddress)
    {
        if (_requestCountPerIpAddress.TryGetValue(ipAddress, out var countSending))
        {
            if (countSending >= MaxRequestPerInterval)
                return Task.FromResult(false);
            _requestCountPerIpAddress[ipAddress] = countSending + 1;
        }

        Task.Delay(_interval).ContinueWith((t) => { _requestCountPerIpAddress.TryRemove(ipAddress, out var _); });
        return Task.FromResult(true);
    }

    public Task<int> AvailableRate(string ipAddress)
    {
        return Task.FromResult(_requestCountPerIpAddress.TryGetValue(ipAddress, out var countSending)
                                   ? countSending
                                   : MaxRequestPerInterval);
    }
}
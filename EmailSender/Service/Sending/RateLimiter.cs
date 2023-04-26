using System.Collections.Concurrent;

namespace MacroMail.Service.Sending;

public class RateLimiter : IRateLimiter
{
    private readonly List<PeriodLimit> _periodLimits = new()
    {
        new PeriodLimit(new TimeOnly(8, 00), new TimeOnly(18, 00), 250),
        new PeriodLimit(new TimeOnly(18, 00), new TimeOnly(8, 00), 500),
    };

    private readonly TimeSpan                          _interval;
    private readonly ConcurrentDictionary<string, int> _requestCountPerIpAddress = new();

    public RateLimiter(TimeSpan interval) { _interval = interval; }

    public Task<bool> TryGetValue(string ipAddress)
    {
        if (_requestCountPerIpAddress.TryGetValue(ipAddress, out var countSending))
        {
            var currentPeriod = _periodLimits.First(p => p.IsInInterval());
            if (countSending >= currentPeriod.Limit)
                return Task.FromResult(false);

            _requestCountPerIpAddress[ipAddress] = countSending + 1;
        }
        else
            _requestCountPerIpAddress.TryAdd(ipAddress, 1);

        Task.Delay(_interval).ContinueWith((t) => { _requestCountPerIpAddress.TryRemove(ipAddress, out var _); });
        return Task.FromResult(true);
    }

    public Task<int> AvailableRate(string ipAddress)
    {
        var currentPeriod = _periodLimits.First(p => p.IsInInterval(DateTime.UtcNow));
        return Task.FromResult(_requestCountPerIpAddress.TryGetValue(ipAddress, out var countSending)
                                   ? currentPeriod.Limit - countSending
                                   : currentPeriod.Limit);
    }
}
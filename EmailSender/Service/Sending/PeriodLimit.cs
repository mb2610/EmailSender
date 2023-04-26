namespace MacroMail.Service.Sending;

public class PeriodLimit
{
    private TimeOnly Start { get; }
    private TimeOnly End   { get; }
    public  int      Limit { get; }

    public PeriodLimit(TimeOnly start, TimeOnly end, int limit)
    {
        Start = start;
        End   = end;
        Limit = limit;
    }

    public bool IsInInterval() => IsInInterval(DateTime.UtcNow);

    public bool IsInInterval(DateTime currentDate)
    {
        var currentTime = TimeOnly.FromDateTime(currentDate);
        if (Start < End)
            return currentTime >= Start && currentTime < End;
        return currentTime >= Start || currentTime < End;
    }
}
namespace KellyCriterion.App.Charting;

public sealed class LogarithmicAxisScale : IAxisScale
{
    private readonly double _logMin;
    private readonly double _logMax;

    public LogarithmicAxisScale(double min, double max)
    {
        min = Math.Max(min, double.Epsilon);
        max = Math.Max(max, min);
        if (max <= min)
        {
            min /= 2;
            max *= 2;
        }

        Min = min;
        Max = max;
        _logMin = Math.Log10(min);
        _logMax = Math.Log10(max);
    }

    public double Min { get; }

    public double Max { get; }

    public double Normalize(double value) => (Math.Log10(Math.Max(value, Min)) - _logMin) / (_logMax - _logMin);

    public IReadOnlyList<double> GetTicks(int maxTickCount)
    {
        var firstDecade = (int)Math.Ceiling(_logMin);
        var lastDecade = (int)Math.Floor(_logMax);
        var decadeCount = lastDecade - firstDecade + 1;

        if (decadeCount < 2)
            return NiceTicks.Between(Min, Max, maxTickCount);

        var step = Math.Max(1, (int)Math.Ceiling(decadeCount / (double)maxTickCount));
        var ticks = new List<double>();
        for (var decade = firstDecade; decade <= lastDecade; decade += step)
            ticks.Add(Math.Pow(10, decade));

        return ticks;
    }
}

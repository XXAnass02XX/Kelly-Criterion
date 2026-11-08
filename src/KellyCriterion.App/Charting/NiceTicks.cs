namespace KellyCriterion.App.Charting;

public static class NiceTicks
{
    public static IReadOnlyList<double> Between(double min, double max, int maxTickCount)
    {
        if (!(max > min) || maxTickCount < 1)
            return [min];

        var step = NiceStep((max - min) / maxTickCount);
        var ticks = new List<double>();
        for (var tick = Math.Ceiling(min / step) * step; tick <= max + step * 1e-9; tick += step)
            ticks.Add(Math.Abs(tick) < step * 1e-9 ? 0 : tick);

        return ticks;
    }

    private static double NiceStep(double roughStep)
    {
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughStep)));
        var residual = roughStep / magnitude;
        var nice = residual switch
        {
            <= 1 => 1,
            <= 2 => 2,
            <= 5 => 5,
            _ => 10,
        };
        return nice * magnitude;
    }
}

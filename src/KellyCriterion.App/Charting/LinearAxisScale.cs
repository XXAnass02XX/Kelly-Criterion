namespace KellyCriterion.App.Charting;

public sealed class LinearAxisScale : IAxisScale
{
    public LinearAxisScale(double min, double max)
    {
        if (max <= min)
        {
            var padding = min == 0 ? 1 : Math.Abs(min) * 0.1;
            min -= padding;
            max += padding;
        }

        Min = min;
        Max = max;
    }

    public double Min { get; }

    public double Max { get; }

    public double Normalize(double value) => (value - Min) / (Max - Min);

    public IReadOnlyList<double> GetTicks(int maxTickCount) => NiceTicks.Between(Min, Max, maxTickCount);
}

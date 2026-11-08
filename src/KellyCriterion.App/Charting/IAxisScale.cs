namespace KellyCriterion.App.Charting;

public interface IAxisScale
{
    double Min { get; }

    double Max { get; }

    double Normalize(double value);

    IReadOnlyList<double> GetTicks(int maxTickCount);
}

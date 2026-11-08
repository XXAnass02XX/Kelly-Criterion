using System.Globalization;
using System.Windows;
using System.Windows.Media;
using KellyCriterion.App.Formatting;

namespace KellyCriterion.App.Charting;

public sealed class WealthChart : FrameworkElement
{
    private const double LeftMargin = 86;
    private const double RightMargin = 24;
    private const double TopMargin = 16;
    private const double BottomMargin = 52;
    private const double TickLength = 5;
    private const double LabelFontSize = 11;
    private const double TitleFontSize = 12;
    private const double SamplesPerPixel = 2;

    private static readonly Color[] RunPalette =
    [
        Color.FromRgb(0x63, 0x66, 0xF1), Color.FromRgb(0xEC, 0x48, 0x99), Color.FromRgb(0x14, 0xB8, 0xA6),
        Color.FromRgb(0xF5, 0x9E, 0x0B), Color.FromRgb(0x8B, 0x5C, 0xF6), Color.FromRgb(0x0E, 0xA5, 0xE9),
        Color.FromRgb(0x10, 0xB9, 0x81), Color.FromRgb(0xF4, 0x3F, 0x5E), Color.FromRgb(0xF9, 0x73, 0x16),
        Color.FromRgb(0x06, 0xB6, 0xD4),
    ];

    private static readonly Typeface LabelTypeface = new("Segoe UI");

    public static readonly DependencyProperty PathsProperty = DependencyProperty.Register(
        nameof(Paths), typeof(IReadOnlyList<double[]>), typeof(WealthChart),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnDataChanged));

    public static readonly DependencyProperty AveragePathProperty = DependencyProperty.Register(
        nameof(AveragePath), typeof(double[]), typeof(WealthChart),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnDataChanged));

    public static readonly DependencyProperty IsLogarithmicProperty = DependencyProperty.Register(
        nameof(IsLogarithmic), typeof(bool), typeof(WealthChart),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnDataChanged));

    public static readonly DependencyProperty RunOpacityProperty = DependencyProperty.Register(
        nameof(RunOpacity), typeof(double), typeof(WealthChart),
        new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty AverageBrushProperty = DependencyProperty.Register(
        nameof(AverageBrush), typeof(Brush), typeof(WealthChart),
        new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty PlotBackgroundProperty = DependencyProperty.Register(
        nameof(PlotBackground), typeof(Brush), typeof(WealthChart),
        new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ReferenceValueProperty = DependencyProperty.Register(
        nameof(ReferenceValue), typeof(double), typeof(WealthChart),
        new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty XAxisTitleProperty = DependencyProperty.Register(
        nameof(XAxisTitle), typeof(string), typeof(WealthChart),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty YAxisTitleProperty = DependencyProperty.Register(
        nameof(YAxisTitle), typeof(string), typeof(WealthChart),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    private (double Min, double Max)? _cachedRange;

    public IReadOnlyList<double[]>? Paths
    {
        get => (IReadOnlyList<double[]>?)GetValue(PathsProperty);
        set => SetValue(PathsProperty, value);
    }

    public double[]? AveragePath
    {
        get => (double[]?)GetValue(AveragePathProperty);
        set => SetValue(AveragePathProperty, value);
    }

    public bool IsLogarithmic
    {
        get => (bool)GetValue(IsLogarithmicProperty);
        set => SetValue(IsLogarithmicProperty, value);
    }

    public double RunOpacity
    {
        get => (double)GetValue(RunOpacityProperty);
        set => SetValue(RunOpacityProperty, value);
    }

    public Brush AverageBrush
    {
        get => (Brush)GetValue(AverageBrushProperty);
        set => SetValue(AverageBrushProperty, value);
    }

    public Brush PlotBackground
    {
        get => (Brush)GetValue(PlotBackgroundProperty);
        set => SetValue(PlotBackgroundProperty, value);
    }

    public double ReferenceValue
    {
        get => (double)GetValue(ReferenceValueProperty);
        set => SetValue(ReferenceValueProperty, value);
    }

    public string XAxisTitle
    {
        get => (string)GetValue(XAxisTitleProperty);
        set => SetValue(XAxisTitleProperty, value);
    }

    public string YAxisTitle
    {
        get => (string)GetValue(YAxisTitleProperty);
        set => SetValue(YAxisTitleProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        var plot = new Rect(
            LeftMargin,
            TopMargin,
            Math.Max(0, ActualWidth - LeftMargin - RightMargin),
            Math.Max(0, ActualHeight - TopMargin - BottomMargin));

        var paths = Paths;
        if (paths is not { Count: > 0 } || paths[0].Length < 2 || plot.Width < 10 || plot.Height < 10)
            return;

        var range = _cachedRange ??= ComputeRange(paths, AveragePath, IsLogarithmic);
        IAxisScale yScale = IsLogarithmic
            ? new LogarithmicAxisScale(range.Min, range.Max)
            : new LinearAxisScale(range.Min, range.Max);
        var xScale = new LinearAxisScale(0, paths[0].Length - 1);
        var mapper = new PlotMapper(plot, xScale, yScale);

        dc.DrawRectangle(PlotBackground, null, plot);
        DrawGridAndAxes(dc, mapper, plot);

        dc.PushClip(new RectangleGeometry(plot));
        DrawReferenceLine(dc, mapper, plot);
        DrawRuns(dc, mapper, paths, plot.Width);
        if (AveragePath is { Length: > 1 } average)
        {
            var averageGeometry = BuildGeometry(average, mapper, plot.Width);
            dc.DrawGeometry(null, CreatePen(Brushes.White, 0.85, 6), averageGeometry);
            dc.DrawGeometry(null, CreatePen(AverageBrush, 1.0, 2.5), averageGeometry);
        }
        dc.Pop();

        DrawAxisTitles(dc, plot);
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((WealthChart)d)._cachedRange = null;

    private static (double Min, double Max) ComputeRange(IReadOnlyList<double[]> paths, double[]? average, bool positiveOnly)
    {
        var min = double.MaxValue;
        var max = double.MinValue;

        void Accumulate(double[] values)
        {
            foreach (var value in values)
            {
                if (!double.IsFinite(value) || (positiveOnly && value <= 0))
                    continue;
                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }
        }

        foreach (var path in paths)
            Accumulate(path);
        if (average is not null)
            Accumulate(average);

        if (min > max)
            return (1, 1);

        if (!positiveOnly)
        {
            var padding = (max - min) * 0.04;
            return (min - padding, max + padding);
        }

        return (min, max);
    }

    private void DrawRuns(DrawingContext dc, PlotMapper mapper, IReadOnlyList<double[]> paths, double plotWidth)
    {
        var pens = RunPalette.Select(c => CreatePen(new SolidColorBrush(c), RunOpacity, 1.0)).ToArray();
        for (var run = 0; run < paths.Count; run++)
            dc.DrawGeometry(null, pens[run % pens.Length], BuildGeometry(paths[run], mapper, plotWidth));
    }

    private void DrawReferenceLine(DrawingContext dc, PlotMapper mapper, Rect plot)
    {
        if (!double.IsFinite(ReferenceValue))
            return;

        var y = mapper.Y(ReferenceValue);
        var pen = CreatePen(new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)), 1.0, 1.2, DashStyles.Dash);
        dc.DrawLine(pen, new Point(plot.Left, y), new Point(plot.Right, y));
    }

    private void DrawGridAndAxes(DrawingContext dc, PlotMapper mapper, Rect plot)
    {
        var gridPen = CreatePen(new SolidColorBrush(Color.FromRgb(0xE8, 0xEB, 0xF8)), 1.0, 1.0);
        var axisPen = CreatePen(new SolidColorBrush(Color.FromRgb(0xA5, 0xB4, 0xFC)), 1.0, 1.0);
        var labelBrush = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B));
        labelBrush.Freeze();

        foreach (var tick in mapper.YScale.GetTicks(Math.Max(2, (int)(plot.Height / 48))))
        {
            var y = mapper.Y(tick);
            if (y < plot.Top - 0.5 || y > plot.Bottom + 0.5)
                continue;

            dc.DrawLine(gridPen, new Point(plot.Left, y), new Point(plot.Right, y));
            dc.DrawLine(axisPen, new Point(plot.Left - TickLength, y), new Point(plot.Left, y));
            var text = CreateText(NumberFormatter.Compact(tick), LabelFontSize, labelBrush);
            dc.DrawText(text, new Point(plot.Left - TickLength - 4 - text.Width, y - text.Height / 2));
        }

        foreach (var tick in mapper.XScale.GetTicks(Math.Max(2, (int)(plot.Width / 90))))
        {
            var x = mapper.X(tick);
            dc.DrawLine(gridPen, new Point(x, plot.Top), new Point(x, plot.Bottom));
            dc.DrawLine(axisPen, new Point(x, plot.Bottom), new Point(x, plot.Bottom + TickLength));
            var text = CreateText(tick.ToString("N0", CultureInfo.CurrentCulture), LabelFontSize, labelBrush);
            dc.DrawText(text, new Point(x - text.Width / 2, plot.Bottom + TickLength + 2));
        }

        dc.DrawLine(axisPen, plot.BottomLeft, plot.BottomRight);
        dc.DrawLine(axisPen, plot.BottomLeft, plot.TopLeft);
    }

    private void DrawAxisTitles(DrawingContext dc, Rect plot)
    {
        var brush = new SolidColorBrush(Color.FromRgb(0x1E, 0x1B, 0x4B));
        brush.Freeze();

        if (!string.IsNullOrEmpty(XAxisTitle))
        {
            var text = CreateText(XAxisTitle, TitleFontSize, brush);
            dc.DrawText(text, new Point(plot.Left + (plot.Width - text.Width) / 2, ActualHeight - text.Height - 4));
        }

        if (!string.IsNullOrEmpty(YAxisTitle))
        {
            var text = CreateText(YAxisTitle, TitleFontSize, brush);
            var origin = new Point(4, plot.Top + (plot.Height + text.Width) / 2);
            dc.PushTransform(new RotateTransform(-90, origin.X, origin.Y));
            dc.DrawText(text, origin);
            dc.Pop();
        }
    }

    private static StreamGeometry BuildGeometry(double[] values, PlotMapper mapper, double plotWidth)
    {
        var maxSamples = Math.Max(2, (int)(plotWidth * SamplesPerPixel));
        var stride = Math.Max(1, (values.Length - 1) / maxSamples);
        var points = new List<Point>(values.Length / stride + 2);

        for (var i = stride; i < values.Length; i += stride)
            points.Add(mapper.Map(i, values[i]));
        if ((values.Length - 1) % stride != 0)
            points.Add(mapper.Map(values.Length - 1, values[^1]));

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(mapper.Map(0, values[0]), isFilled: false, isClosed: false);
            context.PolyLineTo(points, isStroked: true, isSmoothJoin: false);
        }

        geometry.Freeze();
        return geometry;
    }

    private static Pen CreatePen(Brush brush, double opacity, double thickness, DashStyle? dashStyle = null)
    {
        var penBrush = brush.CloneCurrentValue();
        penBrush.Opacity = opacity;

        var pen = new Pen(penBrush, thickness)
        {
            LineJoin = PenLineJoin.Round,
            DashStyle = dashStyle ?? DashStyles.Solid,
        };
        pen.Freeze();
        return pen;
    }

    private FormattedText CreateText(string text, double size, Brush brush) =>
        new(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, LabelTypeface, size, brush,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

    private readonly record struct PlotMapper(Rect Plot, IAxisScale XScale, IAxisScale YScale)
    {
        private const double OverflowLimit = 4;

        public double X(double value) => Plot.Left + Clamp(XScale.Normalize(value)) * Plot.Width;

        public double Y(double value) => Plot.Bottom - Clamp(YScale.Normalize(value)) * Plot.Height;

        public Point Map(double x, double y) => new(X(x), Y(y));

        private static double Clamp(double normalized) =>
            double.IsNaN(normalized) ? 0 : Math.Clamp(normalized, -OverflowLimit, OverflowLimit);
    }
}

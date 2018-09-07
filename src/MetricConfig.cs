namespace MonikTerminal
{
    public class MetricConfigArea
    {
        public double?[] Range { get; set; }
        public string Color { get; set; }
    }

    public class MetricConfigValue
    {
        public long MetricId { get; set; }
        public string ValueFormat { get; set; }
        public MetricConfigArea[] Areas { get; set; }
    }

    public class MetricConfig
    {
        public int MaxMetricLen { get; set; }
        public int MaxMetricValueLen { get; set; }
        public int MaxAggregationTypeLen { get; set; }
        public string DefaultValueFormat { get; set; }
        public MetricConfigValue[] Metrics { get; set; } = { };
    }
}
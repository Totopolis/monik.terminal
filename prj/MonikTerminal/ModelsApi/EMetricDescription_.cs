namespace MonikTerminal.ModelsApi
{
    public class EMetricDescription_
    {
        public long       Id         { get; set; }
        public int        InstanceId { get; set; }
        public string     Name       { get; set; }
        public MetricType Type       { get; set; }
    }

    public enum MetricType
    {
        Accumulator,
        Gauge
    }
}
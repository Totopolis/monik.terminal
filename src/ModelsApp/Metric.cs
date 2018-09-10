using MonikTerminal.Enums;

namespace MonikTerminal.ModelsApp
{
    public class Metric
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public Instance Instance { get; set; }
        public AggregationType Aggregation { get; set; }
    }
}
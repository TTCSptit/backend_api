namespace job.Dtos
{
    public class RecruiterDetailedStatsDto
    {
        public PerformanceChartDto PerformanceChart { get; set; } = new PerformanceChartDto();
        public StatusChartDto StatusChart { get; set; } = new StatusChartDto();
    }

    public class PerformanceChartDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> ApplicationsData { get; set; } = new List<int>();
    }

    public class StatusChartDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<int> Data { get; set; } = new List<int>();
    }
}

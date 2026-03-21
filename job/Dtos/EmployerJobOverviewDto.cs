namespace job.Dtos
{
    public class EmployerJobOverviewDto
    {
        public int TotalPostedJobs { get; set; } = 0;
        public int ActiveJobsCount { get; set; } = 0;
        public int TotalViews { get; set; } = 0;
        public int TotalApplications { get; set; } = 0;

        public List<EmployerJobItemDto> Jobs { get; set; } = new List<EmployerJobItemDto>();
    }
}

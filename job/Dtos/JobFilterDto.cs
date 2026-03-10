namespace job.Dtos
{
    public class JobFilterDto
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public string? CategorySlug { get; set; }
        public int? JobType { get; set; }
        public int? MinSalary { get; set; }
        public int? MaxSalary { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}

namespace job.Dtos
{
    public class UpdateJobDto
    {
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public string? Location { get; set; }

        public int? SalaryMin { get; set; }

        public int? SalaryMax { get; set; }

        public bool IsNegotiable { get; set; }

        public int JobType { get; set; }

        public int CategoryId { get; set; }

        public DateTime? ExpiredAt { get; set; }
    }
}

namespace job.Dtos
{
    public class EmployerJobItemDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Location { get; set; }

        public int? SalaryMin { get; set; }

        public int? SalaryMax { get; set; }

        public int? JobType { get; set; }

        public bool IsNegotiable { get; set; }

        public DateTime? ExpiredAt { get; set; }

        public int ViewCount { get; set; }

        public int CandidateCount { get; set; }
    }
}
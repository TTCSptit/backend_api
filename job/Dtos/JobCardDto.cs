using job.Models;

namespace job.Dtos
{
    public class JobCardDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string CompanyName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyLogoUrl { get; set; }

        public string Location { get; set; }

        public int? SalaryMin { get; set; }

        public int? SalaryMax { get; set; }

        public string? Description { get; set; }

        public int CategoryId { get; set; }

        public int? JobType { get; set; }

        public bool IsNegotiable { get; set; }

        public DateTime? ExpiredAt { get; set; }
    }
}

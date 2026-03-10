using job.Models;

namespace job.Dtos
{
    public class CompanyDetailDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Location { get; set; }

        public string? LogoUrl { get; set; }

        public string? Description { get; set; }

        public bool IsVerified { get; set; }

        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}

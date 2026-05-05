using job.Models;

namespace job.Dtos
{
    public class UpdateCompanyRequestDto
    {
        public string Name { get; set; } = null!;

        public string? Location { get; set; }

        public string? WebsiteUrl { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? LogoUrl { get; set; }

        public string? Description { get; set; }
        public string? Industry { get; set; }
        public string? Size { get; set; }
        public string? Founded { get; set; }
    }
}

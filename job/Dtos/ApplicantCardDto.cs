using job.Models;

namespace job.Dtos
{
    public class ApplicantCardDto
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public DateTime AppliedAt { get; set; }

        public string FullName { get; set; }

        public string? Phone { get; set; }

        public string? Location { get; set; }

        public string? AboutMe { get; set; }

        public string? Cvurl { get; set; }

        public List<string> Skills { get; set; }

        public  List<Education> Educations { get; set; } = new List<Education>();

        public  List<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();


    }
}
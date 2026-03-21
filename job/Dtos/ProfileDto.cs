using job.Models;

namespace job.Dtos
{
    public class ProfileDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Location { get; set; }

        public string? AboutMe { get; set; }

        public string? Cvurl { get; set; }

        public List<EducationDto> Educations { get; set; } = new List<EducationDto>();

        public  List<WorkExperienceDto> WorkExperiences { get; set; } = new List<WorkExperienceDto>();

        public  List<SkillDto> Skills { get; set; } = new List<SkillDto>();
    }
}

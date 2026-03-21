namespace job.Dtos
{
    public class WorkExperienceDto
    {
        public int? Id { get; set; }
        public string CompanyName { get; set; } = null!;

        public string Position { get; set; } = null!;

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public string? Description { get; set; }
    }
}

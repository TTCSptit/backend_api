namespace job.Dtos
{
    public class EducationDto
    {
        public int? Id { get; set; }
        public string SchoolName { get; set; } = null!;

        public string? Degree { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}

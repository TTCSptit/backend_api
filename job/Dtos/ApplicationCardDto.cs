namespace job.Dtos
{
    public class ApplicationCardDto
    {
        public int Id { get; set; }
        public JobCardDto JobCardDto { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime AppliedAt { get; set; }
    }
}

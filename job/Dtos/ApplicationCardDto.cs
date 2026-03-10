namespace job.Dtos
{
    public class ApplicationCardDto
    {
        public JobCardDto JobCardDto { get; set; }
        public int Status { get; set; }

        public DateTime AppliedAt { get; set; }
    }
}

namespace job.Dtos
{
    public class UpdateApplicationStatusDto
    {
        public int ApplicationId { get; set; }
        public string NewStatus { get; set; } = null!;
    }
}

namespace job.Dtos
{
    public class UpdateAiScoreDto
    {
        public int ApplicationId { get; set; }
        public int AIScore { get; set; }
        public string? AIStrengths { get; set; }
        public string? AIWeaknesses { get; set; }
        public string? AIReasoning { get; set; }
    }
}

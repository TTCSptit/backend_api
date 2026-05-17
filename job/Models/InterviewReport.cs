using System;

namespace job.Models
{
    public class InterviewReport
    {
        public int Id { get; set; }
        public string RoomId { get; set; } = null!;
        public int CommunicationScore { get; set; }
        public int TechnicalScore { get; set; }
        public int ConfidenceScore { get; set; }
        public string FeedbackStrengths { get; set; } = null!; // JSON array string
        public string FeedbackWeaknesses { get; set; } = null!; // JSON array string
        public string TranscriptSummary { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

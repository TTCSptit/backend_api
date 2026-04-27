using System;

namespace job.Models
{
    public class AiChatSession
    {
        public string Id { get; set; } = string.Empty; // SessionId từ AI Service
        public string Title { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    }
}

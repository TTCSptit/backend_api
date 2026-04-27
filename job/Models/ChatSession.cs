using System;

namespace job.Models
{
    public class ChatSession
    {
        public string Id { get; set; } = string.Empty;
        public string User1Id { get; set; } = string.Empty;
        public string User2Id { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    }
}

using System;

namespace job.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; } // "image", "file", "voice"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}

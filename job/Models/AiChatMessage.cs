using System;

namespace job.Models
{
    public class AiChatMessage
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "user" hoặc "ai"
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Dữ liệu bổ sung từ AI (JSON string) nếu cần hiển thị lại Dashboard
        public string? AiDataJson { get; set; }
    }
}

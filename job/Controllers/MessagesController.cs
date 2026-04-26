using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using job.Models;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace job.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private static readonly List<ChatMessage> _mockMessages = new List<ChatMessage>
        {
            new ChatMessage { Id = 1, SenderId = "recruiter1", ReceiverId = "candidate1", Message = "Chào bạn, mình thấy hồ sơ của bạn rất ấn tượng!", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new ChatMessage { Id = 2, SenderId = "candidate1", ReceiverId = "recruiter1", Message = "Dạ em chào anh, em cảm ơn anh đã quan tâm ạ.", Timestamp = DateTime.UtcNow.AddHours(-1) }
        };

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Return unique contacts
            var contacts = _mockMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .Select(id => new { Id = id, Name = "User " + id })
                .ToList();
            
            return Ok(ApiResponse<object>.SuccessResponse(contacts));
        }

        [HttpGet("{contactId}")]
        public async Task<IActionResult> GetMessages(string contactId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var messages = _mockMessages
                .Where(m => (m.SenderId == userId && m.ReceiverId == contactId) || (m.SenderId == contactId && m.ReceiverId == userId))
                .OrderBy(m => m.Timestamp)
                .ToList();

            return Ok(ApiResponse<List<ChatMessage>>.SuccessResponse(messages));
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var newMessage = new ChatMessage
            {
                Id = _mockMessages.Count + 1,
                SenderId = userId,
                ReceiverId = dto.ReceiverId,
                Message = dto.Message,
                Timestamp = DateTime.UtcNow
            };
            _mockMessages.Add(newMessage);
            return Ok(ApiResponse<ChatMessage>.SuccessResponse(newMessage));
        }
    }

    public class SendMessageDto {
        public string ReceiverId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

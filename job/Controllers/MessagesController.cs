// DEBUG: Updated by AI - Fixed Persistence by using DB instead of Mock
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using job.Models;
using job.Dtos;
using job.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace job.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly JobPtitContext _context;

        public MessagesController(JobPtitContext context)
        {
            _context = context;
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Lấy ID của những người đã nhắn tin với user hiện tại
            var contactIds = await _context.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            // Lấy thông tin chi tiết (tên) của các contact đó
            var contacts = await _context.Users
                .Where(u => contactIds.Contains(u.Id))
                .Select(u => new { 
                    Id = u.Id, 
                    Name = u.FullName ?? u.UserName ?? "Người dùng",
                    LastMessage = _context.ChatMessages
                        .Where(m => (m.SenderId == userId && m.ReceiverId == u.Id) || (m.SenderId == u.Id && m.ReceiverId == userId))
                        .OrderByDescending(m => m.Timestamp)
                        .Select(m => m.Message)
                        .FirstOrDefault(),
                    Time = _context.ChatMessages
                        .Where(m => (m.SenderId == userId && m.ReceiverId == u.Id) || (m.SenderId == u.Id && m.ReceiverId == userId))
                        .OrderByDescending(m => m.Timestamp)
                        .Select(m => m.Timestamp)
                        .FirstOrDefault()
                })
                .OrderByDescending(c => c.Time)
                .ToListAsync();
            
            return Ok(ApiResponse<object>.SuccessResponse(contacts));
        }

        [HttpGet("{contactId}")]
        public async Task<IActionResult> GetMessages(string contactId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == userId && m.ReceiverId == contactId) || (m.SenderId == contactId && m.ReceiverId == userId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return Ok(ApiResponse<List<ChatMessage>>.SuccessResponse(messages));
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var newMessage = new ChatMessage
            {
                SenderId = userId,
                ReceiverId = dto.ReceiverId,
                Message = dto.Message,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(newMessage);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<ChatMessage>.SuccessResponse(newMessage));
        }
    }

    public class SendMessageDto {
        public string ReceiverId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}


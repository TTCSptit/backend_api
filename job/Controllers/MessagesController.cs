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

            // Lấy thông tin chi tiết (tên, email) của các contact đó
            var contacts = await _context.Users
                .Where(u => contactIds.Contains(u.Id))
                .Select(u => new {
                    Id = u.Id,
                    Name = u.FullName ?? u.UserName ?? "Người dùng",
                    Email = u.Email,
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
                .Where(m => (m.SenderId == userId && m.ReceiverId == contactId) ||
                            (m.SenderId == contactId && m.ReceiverId == userId))
                .OrderBy(m => m.Timestamp)
                .Join(_context.Users,
                      m => m.SenderId,
                      u => u.Id,
                      (m, u) => new ChatMessageDto
                      {
                          Id = m.Id,
                          SenderId = m.SenderId,
                          SenderEmail = u.Email ?? string.Empty,
                          SenderName = u.FullName ?? u.UserName ?? "Người dùng",
                          ReceiverId = m.ReceiverId,
                          Message = m.Message,
                          Timestamp = m.Timestamp,
                          IsRead = m.IsRead
                      })
                .ToListAsync();

            return Ok(ApiResponse<List<ChatMessageDto>>.SuccessResponse(messages));
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var sender = await _context.Users.FindAsync(userId);
            if (sender == null) return Unauthorized();

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

            // Trả về DTO đầy đủ thay vì model thô
            var responseDto = new ChatMessageDto
            {
                Id = newMessage.Id,
                SenderId = newMessage.SenderId,
                SenderEmail = sender.Email ?? string.Empty,
                SenderName = sender.FullName ?? sender.UserName ?? "Người dùng",
                ReceiverId = newMessage.ReceiverId,
                Message = newMessage.Message,
                Timestamp = newMessage.Timestamp,
                IsRead = newMessage.IsRead
            };

            return Ok(ApiResponse<ChatMessageDto>.SuccessResponse(responseDto));
        }
    }

    public class SendMessageDto
    {
        public string ReceiverId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

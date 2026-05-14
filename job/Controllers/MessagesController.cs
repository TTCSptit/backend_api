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
        private readonly IWebHostEnvironment _environment;

        public MessagesController(JobPtitContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
                          AttachmentUrl = m.AttachmentUrl,
                          AttachmentType = m.AttachmentType,
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

        [HttpPost("send-attachment")]
        public async Task<IActionResult> SendAttachment([FromForm] string receiverId, [FromForm] string? message, [FromForm] IFormFile file, [FromForm] string type)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var sender = await _context.Users.FindAsync(userId);
            if (sender == null) return Unauthorized();

            string wwwRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(wwwRootPath, "uploads", "messages");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachmentUrl = $"/uploads/messages/{fileName}";

            var newMessage = new ChatMessage
            {
                SenderId = userId,
                ReceiverId = receiverId,
                Message = message ?? string.Empty,
                AttachmentUrl = attachmentUrl,
                AttachmentType = type,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(newMessage);
            await _context.SaveChangesAsync();

            var responseDto = new ChatMessageDto
            {
                Id = newMessage.Id,
                SenderId = newMessage.SenderId,
                SenderEmail = sender.Email ?? string.Empty,
                SenderName = sender.FullName ?? sender.UserName ?? "Người dùng",
                ReceiverId = newMessage.ReceiverId,
                Message = newMessage.Message,
                AttachmentUrl = newMessage.AttachmentUrl,
                AttachmentType = newMessage.AttachmentType,
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

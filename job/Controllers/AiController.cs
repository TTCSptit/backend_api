using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using job.Data;
using job.Models;
using System.Text;
using System.Text.Json;
using System.Net.WebSockets;

namespace job.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JobPtitContext _context;
        private readonly string _aiServiceUrl = "https://kakakaak123-ai-career-advisor.hf.space/api";

        public AiController(IHttpClientFactory httpClientFactory, JobPtitContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        /// <summary>
        /// Proxy cho Chat Streaming (SSE)
        /// </summary>
        [HttpPost("chat")]
        public async Task Chat()
        {
            var client = _httpClientFactory.CreateClient();
            
            var content = new MultipartFormDataContent();
            
            // Forward các field text (message, session_id, user_id)
            foreach (var key in Request.Form.Keys)
            {
                content.Add(new StringContent(Request.Form[key]!), key);
            }

            // Forward file CV nếu có
            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files[0];
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "cv_file", file.FileName);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_aiServiceUrl}/chat")
            {
                Content = content
            };

            // Lấy thông tin user để lưu lịch sử
            var userId = Request.Form["user_id"].ToString();
            var messageText = Request.Form["message"].ToString();
            var sessionId = Request.Form["session_id"].ToString();

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(messageText))
            {
                try
                {
                    // Quản lý AiChatSession
                    var session = await _context.AiChatSessions.FindAsync(sessionId);
                    if (session == null && !string.IsNullOrEmpty(sessionId))
                    {
                        session = new AiChatSession
                        {
                            Id = sessionId,
                            UserId = userId,
                            Title = messageText.Length > 50 ? messageText.Substring(0, 47) + "..." : messageText,
                            CreatedAt = DateTime.UtcNow,
                            LastMessageAt = DateTime.UtcNow
                        };
                        _context.AiChatSessions.Add(session);
                    }
                    else if (session != null)
                    {
                        session.LastMessageAt = DateTime.UtcNow;
                    }

                    _context.AiChatMessages.Add(new AiChatMessage
                    {
                        UserId = userId,
                        SessionId = sessionId,
                        Role = "user",
                        Message = messageText,
                        Timestamp = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log error but continue chat
                    Console.WriteLine($"Error saving user message: {ex.Message}");
                }
            }

            // Thiết lập timeout dài hơn cho AI processing
            client.Timeout = TimeSpan.FromMinutes(5);

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            
            if (response.IsSuccessStatusCode)
            {
                Response.ContentType = "text/event-stream";
                Response.Headers.CacheControl = "no-cache";
                Response.Headers.Connection = "keep-alive";
                
                var responseStream = await response.Content.ReadAsStreamAsync();
                var reader = new StreamReader(responseStream);
                var fullAiResponse = new StringBuilder();
                var aiDataJson = string.Empty;
                var isReadingData = false;

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line)) continue;

                    // Gửi line về cho client ngay lập tức
                    await Response.WriteAsync(line + "\n\n");
                    await Response.Body.FlushAsync();

                    if (line.StartsWith("data: "))
                    {
                        var data = line.Substring(6).Trim();
                        if (data == "[DONE]") break;
                        if (data == "---DATA---")
                        {
                            isReadingData = true;
                            continue;
                        }

                        if (isReadingData)
                        {
                            // Đây là JSON data cho dashboard
                            aiDataJson = data;
                            isReadingData = false;
                        }
                        else
                        {
                            // Ghép text chunk
                            var textChunk = data.Replace("\\n", "\n");
                            fullAiResponse.Append(textChunk);
                        }
                    }
                }

                // Lưu phản hồi của AI vào DB sau khi kết thúc stream
                if (!string.IsNullOrEmpty(userId) && fullAiResponse.Length > 0)
                {
                    try
                    {
                        _context.AiChatMessages.Add(new AiChatMessage
                        {
                            UserId = userId,
                            SessionId = sessionId,
                            Role = "ai",
                            Message = fullAiResponse.ToString(),
                            Timestamp = DateTime.UtcNow,
                            AiDataJson = aiDataJson
                        });
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving AI response: {ex.Message}");
                    }
                }
            }
            else
            {
                Response.StatusCode = (int)response.StatusCode;
                var errorMsg = await response.Content.ReadAsStringAsync();
                await Response.WriteAsync(errorMsg);
            }
        }

        /// <summary>
        /// Proxy lấy dữ liệu kỹ năng cho Radar Chart
        /// </summary>
        [HttpGet("skills/{userId}")]
        public async Task<IActionResult> GetSkills(string userId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_aiServiceUrl}/skills/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            
            return StatusCode((int)response.StatusCode);
        }

        /// <summary>
        /// Lấy lịch sử chat từ Database .NET
        /// </summary>
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetHistory(string userId)
        {
            var messages = _context.AiChatMessages
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.Timestamp)
                .ToList();

            return Ok(new { user_id = userId, messages = messages });
        }

        /// <summary>
        /// Lấy danh sách các phiên chat với AI của người dùng
        /// </summary>
        [HttpGet("sessions/{userId}")]
        public async Task<IActionResult> GetSessions(string userId)
        {
            var sessions = _context.AiChatSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.LastMessageAt)
                .ToList();

            return Ok(sessions);
        }

        /// <summary>
        /// Kết nối WebSocket Proxy cho Chat
        /// </summary>
        [Route("ws-chat/{userId}")]
        public async Task ConnectChat(string userId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await HandleAiWebSocketProxy(webSocket, userId);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task HandleAiWebSocketProxy(WebSocket frontendSocket, string userId)
        {
            using var aiSocket = new ClientWebSocket();
            // Thêm /api vào trước /ws để khớp với APIRouter trong Python
            var aiUri = new Uri($"wss://kakakaak123-ai-career-advisor.hf.space/api/ws/chat/{userId}");

            try
            {
                // Thêm Origin header để tránh lỗi 403 khi kết nối tới Hugging Face Spaces
                aiSocket.Options.SetRequestHeader("Origin", "https://kakakaak123-ai-career-advisor.hf.space");
                
                await aiSocket.ConnectAsync(aiUri, CancellationToken.None);

                // Task nhận từ AI -> Frontend
                var receiveTask = ProxyAiToFrontend(aiSocket, frontendSocket, userId);
                // Task gửi từ Frontend -> AI
                var sendTask = ProxyFrontendToAi(frontendSocket, aiSocket, userId);

                await Task.WhenAny(receiveTask, sendTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket Proxy Error: {ex.Message}");
            }
            finally
            {
                if (aiSocket.State == WebSocketState.Open)
                    await aiSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
        }

        private async Task ProxyFrontendToAi(WebSocket source, WebSocket destination, string userId)
        {
            var buffer = new byte[1024 * 4];
            while (source.State == WebSocketState.Open && destination.State == WebSocketState.Open)
            {
                var result = await source.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    try
                    {
                        var data = JsonDocument.Parse(messageJson);
                        var text = data.RootElement.GetProperty("message").GetString();
                        var sessionId = data.RootElement.TryGetProperty("session_id", out var sid) ? sid.GetString() : "default";

                        if (!string.IsNullOrEmpty(text))
                        {
                            // Lưu vào DB
                            var session = await _context.AiChatSessions.FindAsync(sessionId);
                            if (session == null)
                            {
                                session = new AiChatSession
                                {
                                    Id = sessionId!,
                                    UserId = userId,
                                    Title = text.Length > 50 ? text.Substring(0, 47) + "..." : text,
                                    CreatedAt = DateTime.UtcNow,
                                    LastMessageAt = DateTime.UtcNow
                                };
                                _context.AiChatSessions.Add(session);
                            }
                            else
                            {
                                session.LastMessageAt = DateTime.UtcNow;
                            }

                            _context.AiChatMessages.Add(new AiChatMessage
                            {
                                UserId = userId,
                                SessionId = sessionId!,
                                Role = "user",
                                Message = text,
                                Timestamp = DateTime.UtcNow
                            });
                            await _context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("WS Send Log Error: " + ex.Message); }
                }

                await destination.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
            }
        }

        private async Task ProxyAiToFrontend(WebSocket source, WebSocket destination, string userId)
        {
            var buffer = new byte[1024 * 4];
            var aiResponseBuffer = new StringBuilder();
            var lastSessionId = "default";

            while (source.State == WebSocketState.Open && destination.State == WebSocketState.Open)
            {
                var result = await source.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var responseJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    try
                    {
                        var data = JsonDocument.Parse(responseJson);
                        var type = data.RootElement.GetProperty("type").GetString();
                        
                        if (type == "content")
                        {
                            aiResponseBuffer.Append(data.RootElement.GetProperty("content").GetString());
                        }
                        else if (type == "end")
                        {
                            var sessionId = data.RootElement.GetProperty("session_id").GetString();
                            var aiDataJson = data.RootElement.TryGetProperty("data", out var d) ? d.ToString() : null;

                            // Lưu phản hồi của AI vào DB
                            _context.AiChatMessages.Add(new AiChatMessage
                            {
                                UserId = userId,
                                SessionId = sessionId!,
                                Role = "ai",
                                Message = aiResponseBuffer.ToString(),
                                Timestamp = DateTime.UtcNow,
                                AiDataJson = aiDataJson
                            });
                            await _context.SaveChangesAsync();
                            aiResponseBuffer.Clear();
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("WS Receive Log Error: " + ex.Message); }
                }

                await destination.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
            }
        }
    }
}

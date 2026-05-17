using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http.Headers;

namespace job.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _aiServiceUrl = "http://127.0.0.1:8000/api";

        public WebhooksController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Webhook nhận thông báo ghi âm từ ZegoCloud
        /// </summary>
        [HttpPost("zegocloud")]
        public async Task<IActionResult> ZegoCloudCallback([FromBody] JsonElement payload)
        {
            try
            {
                // In log để debug cấu trúc ZegoCloud gửi về
                Console.WriteLine($"[ZegoCloud Webhook] Nhận được callback: {payload.GetRawText()}");

                // Thường ZegoCloud trả về room_id và mảng file_list chứa file_url
                // Dưới đây là logic parse minh họa (tùy thuộc vào config OSS trong ZegoCloud)
                string roomId = "";
                string fileUrl = "";

                if (payload.TryGetProperty("room_id", out var roomIdProp))
                {
                    roomId = roomIdProp.GetString() ?? "";
                }

                if (payload.TryGetProperty("file_list", out var fileListProp) && fileListProp.GetArrayLength() > 0)
                {
                    var firstFile = fileListProp[0];
                    if (firstFile.TryGetProperty("file_url", out var fileUrlProp))
                    {
                        fileUrl = fileUrlProp.GetString() ?? "";
                    }
                }

                // Nếu không có file_url trực tiếp, có thể giả lập để test
                if (string.IsNullOrEmpty(fileUrl))
                {
                    Console.WriteLine("[ZegoCloud Webhook] Không tìm thấy file_url, có thể do chưa config OSS lưu trữ.");
                    // Test purpose: mock a url if testing locally
                    // fileUrl = "https://example.com/test_recording.mp3";
                }

                if (!string.IsNullOrEmpty(fileUrl) && !string.IsNullOrEmpty(roomId))
                {
                    // Chuyển tiếp file URL sang AI Service để bóc băng (STT) và phân tích
                    var client = _httpClientFactory.CreateClient();
                    
                    var requestBody = new
                    {
                        room_id = roomId,
                        audio_url = fileUrl
                    };

                    var content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
                    
                    // Fire and forget (Hoặc cho vào RabbitMQ để AI xử lý ngầm)
                    _ = client.PostAsync($"{_aiServiceUrl}/interview/analyze-audio", content);
                    
                    Console.WriteLine($"[ZegoCloud Webhook] Đã đẩy request phân tích audio cho phòng {roomId} sang AI Service.");
                }

                return Ok(new { error_code = 0, error_message = "success" }); // ZegoCloud yêu cầu trả về code 0
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ZegoCloud Webhook Lỗi]: {ex.Message}");
                return StatusCode(500, new { error_code = 1, error_message = ex.Message });
            }
        }
    }
}

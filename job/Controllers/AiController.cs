using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;

namespace job.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _aiServiceUrl = "http://localhost:8000/api";

        public AiController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

            // Thiết lập timeout dài hơn cho AI processing
            client.Timeout = TimeSpan.FromMinutes(5);

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            
            if (response.IsSuccessStatusCode)
            {
                Response.ContentType = "text/event-stream";
                Response.Headers.CacheControl = "no-cache";
                Response.Headers.Connection = "keep-alive";
                
                var responseStream = await response.Content.ReadAsStreamAsync();
                
                // Copy stream trực tiếp về client để hỗ trợ Streaming
                await responseStream.CopyToAsync(Response.Body);
                await Response.Body.FlushAsync();
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
        /// Proxy lấy lịch sử chat
        /// </summary>
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetHistory(string userId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_aiServiceUrl}/history/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            
            return StatusCode((int)response.StatusCode);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using job.Models;
using job.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace job.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        // Mock data for now since we can't run migrations
        private static readonly List<News> _mockNews = new List<News>
        {
            new News { 
                Id = 1, 
                Title = "Bí quyết viết CV ấn tượng cho sinh viên PTIT", 
                Summary = "Hướng dẫn chi tiết cách trình bày các dự án đồ án vào CV để thu hút nhà tuyển dụng.",
                Content = "...", 
                ImageUrl = "https://ptit.edu.vn/wp-content/uploads/2021/07/bg-header.png",
                Author = "Phòng CTCT&SV"
            },
            new News { 
                Id = 2, 
                Title = "Top 10 kỹ năng lập trình hot nhất năm 2026", 
                Summary = "Phân tích xu hướng thị trường lao động trong lĩnh vực CNTT tại Việt Nam.",
                Content = "...", 
                ImageUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97",
                Author = "Admin"
            }
        };

        [HttpGet]
        public async Task<IActionResult> GetNews()
        {
            return Ok(ApiResponse<List<News>>.SuccessResponse(_mockNews));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsById(int id)
        {
            var news = _mockNews.FirstOrDefault(n => n.Id == id);
            if (news == null) return NotFound(ApiResponse<object>.FailureResponse("News not found"));
            return Ok(ApiResponse<News>.SuccessResponse(news));
        }
    }
}

using job.Dtos;
using job.Services;
using Microsoft.AspNetCore.Mvc;

namespace job.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] string? keyword)
        {
            var categories = await _categoryService.GetCategories(keyword);

            return Ok(ApiResponse<List<CategoryCardDto>>.SuccessResponse(categories));
        }

        [HttpGet("Featured")]
        public async Task<IActionResult> GetFeaturedCategories([FromQuery] int count = 6, [FromQuery] int days = 30)
        {
            var featured = await _categoryService.GetFeaturedCategories(count, days);

            if (featured == null || !featured.Any())
            {
                return NotFound(ApiResponse<object>.FailureResponse("No featured categories found for the specified period."));
            }

            return Ok(ApiResponse<List<FeaturedCategoryCardDto>>.SuccessResponse(featured));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetCategory(int id)
        {
            return Ok();
        }
    }
}
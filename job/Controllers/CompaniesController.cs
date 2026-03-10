using Microsoft.AspNetCore.Mvc;

namespace job.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        [HttpGet("{id:int}")]
        public IActionResult GetCompany(int id)
        {
            return Ok();
        }
    }
}

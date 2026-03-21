using job.Filters;
using System.ComponentModel.DataAnnotations;

namespace job.Dtos
{
    public class FileUploadDto
    {
        [Required]
        [AllowedExtensions(new string[] { ".pdf", ".doc", ".docx" })]
        public IFormFile Cv { get; set; }
    }
}

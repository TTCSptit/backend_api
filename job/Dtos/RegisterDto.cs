using System.ComponentModel.DataAnnotations;

namespace job.Dtos
{
    public class RegisterDto
    {
        public string FullName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string? CompanyName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

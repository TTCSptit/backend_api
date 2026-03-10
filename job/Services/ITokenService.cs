using job.Models;

namespace job.Services
{
    public interface ITokenService
    {
        string CreateJwt(ApplicationUser user, string role);
    }
}

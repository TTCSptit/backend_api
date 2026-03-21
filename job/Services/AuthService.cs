using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Web;
using job.Configurations;
using job.Data;
using job.Dtos;
using job.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Google.Apis.Auth.GoogleJsonWebSignature;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;
using AuthResult = job.Dtos.AuthResult;

namespace job.Services
{
    public class AuthService : IAuthService
    {
        private readonly JobPtitContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleSettings _roleSettings;
        private readonly ITokenService _tokenService;

        public AuthService(JobPtitContext context, UserManager<ApplicationUser> userManager, IOptions<RoleSettings> options, ITokenService tokenService)
        {
            _context = context;
            _userManager = userManager;
            _roleSettings = options.Value;
            _tokenService = tokenService;
        }

        public async Task<AuthResult> ExternalLoginAsync(Payload payload, string role)
        {
            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Email = payload.Email,
                    UserName = payload.Email.Split('@')[0],
                    FullName = payload.Name,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded) return AuthResult.Error(AuthResultStatus.Failure, "User creation failed.");

                await _userManager.AddToRoleAsync(user, role);

                if (role.Equals("Candidate", StringComparison.OrdinalIgnoreCase))
                {
                    var profile = new CandidateProfile
                    {
                        UserId = user.Id,
                        FullName = payload.Name,
                        Email = payload.Email,  
                        AboutMe = ""
                    };
                    _context.CandidateProfiles.Add(profile);
                    await _context.SaveChangesAsync();
                }
            }

            var token = _tokenService.CreateJwt(user, role);
            return AuthResult.Ok(new { Token = token, User = user });
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new ApplicationUser
                {
                    Email = dto.Email,
                    UserName = dto.Email.Split('@')[0],
                    FullName = dto.FullName,
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                    return AuthResult.Error(AuthResultStatus.ValidationError, string.Join(", ", result.Errors.Select(e => e.Description)));

                if (string.IsNullOrEmpty(dto.CompanyName))
                {
                    await _userManager.AddToRoleAsync(user, _roleSettings.DefaultRole);

                    var profile = new CandidateProfile
                    {
                        UserId = user.Id,
                        FullName = dto.FullName,
                        Email = dto.Email,
                        AboutMe = "",
                    };
                    _context.CandidateProfiles.Add(profile);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, _roleSettings.EmployerRole);

                    var company = new Company
                    {
                        Name = dto.CompanyName,
                        OwnerUserId = user.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Companies.Add(company);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return AuthResult.Ok(new { user.Email, user.FullName });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return AuthResult.Error(AuthResultStatus.Failure, "Failed when registering user: " + ex.Message);
            }
        }
    }
}

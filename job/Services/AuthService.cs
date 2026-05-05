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
using Microsoft.Extensions.Caching.Memory;

namespace job.Services
{
    public class AuthService : IAuthService
    {
        private readonly JobPtitContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleSettings _roleSettings;
        private readonly ITokenService _tokenService;
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;

        public AuthService(JobPtitContext context, UserManager<ApplicationUser> userManager, IOptions<RoleSettings> options, ITokenService tokenService, IMemoryCache cache, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _roleSettings = options.Value;
            _tokenService = tokenService;
            _cache = cache;
            _emailService = emailService;
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

        public async Task<AuthResult> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return AuthResult.Error(AuthResultStatus.NotFound, "User not found.");

            var otp = new Random().Next(100000, 999999).ToString();
            _cache.Set($"OTP_{email}", otp, TimeSpan.FromMinutes(15));

            var subject = "PTIT Jobs - Mã xác nhận khôi phục mật khẩu";
            var body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                    <h2 style='color: #e11d48;'>Khôi phục mật khẩu PTIT Jobs</h2>
                    <p>Chào <strong>{user.FullName}</strong>,</p>
                    <p>Bạn đã yêu cầu khôi phục mật khẩu. Mã OTP của bạn là:</p>
                    <div style='background: #f3f4f6; padding: 15px; font-size: 24px; font-weight: bold; letter-spacing: 5px; text-align: center; border-radius: 8px;'>
                        {otp}
                    </div>
                    <p>Mã này có hiệu lực trong vòng <strong>15 phút</strong>. Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #777;'>Đây là email tự động, vui lòng không phản hồi.</p>
                </div>";

            await _emailService.SendEmailAsync(email, subject, body);
            return AuthResult.Ok(null, "OTP has been sent to your email.");
        }

        public async Task<AuthResult> VerifyOtpAsync(string email, string otp)
        {
            if (_cache.TryGetValue($"OTP_{email}", out string? cachedOtp) && cachedOtp == otp)
            {
                return AuthResult.Ok(null, "OTP verified successfully.");
            }
            return AuthResult.Error(AuthResultStatus.ValidationError, "Invalid or expired OTP.");
        }

        public async Task<AuthResult> ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (!_cache.TryGetValue($"OTP_{dto.Email}", out string? cachedOtp) || cachedOtp != dto.Otp)
            {
                return AuthResult.Error(AuthResultStatus.ValidationError, "Invalid or expired OTP.");
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return AuthResult.Error(AuthResultStatus.NotFound, "User not found.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);

            if (result.Succeeded)
            {
                _cache.Remove($"OTP_{dto.Email}");
                return AuthResult.Ok(null, "Password reset successfully.");
            }

            return AuthResult.Error(AuthResultStatus.ValidationError, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}

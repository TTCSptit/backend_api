using Azure.Core;
using Google.Apis.Auth;
using job.Configurations;
using job.Dtos;
using job.Models;
using job.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace job.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IConfiguration configuration, IAuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _configuration = configuration;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
                return Unauthorized(ApiResponse<object>.FailureResponse("Invalid credentials."));


            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, true);

            if (signInResult.IsLockedOut)
                return BadRequest(ApiResponse<object>.FailureResponse("Account is locked due to multiple failed attempts."));


            if (signInResult.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Any(r => r.Equals(dto.Role, StringComparison.OrdinalIgnoreCase)))
                {
                    return Ok(ApiResponse<object>.SuccessResponse(new
                    {
                        Token = _tokenService.CreateJwt(user, dto.Role),
                        User = new { user.Id, user.Email, user.UserName, user.FullName },
                    }, "Login successfully"));
                }
                else
                    return StatusCode(403, ApiResponse<object>.FailureResponse("Your account does not have access to this section."));
            }

            return Unauthorized(ApiResponse<object>.FailureResponse("Invalid password."));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (result.Succeeded)
                return Ok(ApiResponse<object>.SuccessResponse(result.Data));
            

            return result.Status switch
            {
                AuthResultStatus.Conflict => Conflict(ApiResponse<object>.FailureResponse(result.Message)),
                AuthResultStatus.ValidationError => UnprocessableEntity(ApiResponse<object>.FailureResponse(result.Message)),
                AuthResultStatus.Failure => StatusCode(500, ApiResponse<object>.FailureResponse(result.Message)),
                _ => BadRequest(ApiResponse<object>.FailureResponse(result.Message))
            };
        }

        [HttpPost("login-google")]
        public async Task<IActionResult> GoogleLoginAsync([FromBody] GoogleLoginRequestDto dto)
        {
            Payload payload;
            try
            {
                payload = await ValidateAsync(dto.IdToken, new ValidationSettings
                {
                    Audience = new[] { _configuration["OAuth:Google:ClientId"] }
                });
            }
            catch
            {
                return BadRequest(ApiResponse<object>.FailureResponse("Google Token invalid."));
            }

            var result = await _authService.ExternalLoginAsync(payload, dto.Role);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.FailureResponse(result.Message));

            return Ok(ApiResponse<object>.SuccessResponse(result.Data, "Login successful"));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto.Email);
            if (result.Succeeded) return Ok(ApiResponse<object>.SuccessResponse(null, result.Message));
            return result.Status == AuthResultStatus.NotFound ? NotFound(ApiResponse<object>.FailureResponse(result.Message)) : BadRequest(ApiResponse<object>.FailureResponse(result.Message));
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var result = await _authService.VerifyOtpAsync(dto.Email, dto.Otp);
            if (result.Succeeded) return Ok(ApiResponse<object>.SuccessResponse(null, result.Message));
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            if (result.Succeeded) return Ok(ApiResponse<object>.SuccessResponse(null, result.Message));
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message));
        }
    }
}

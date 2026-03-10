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
        private readonly RoleSettings _roleSettings;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<RoleSettings> options, ITokenService tokenService, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleSettings = options.Value;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("Invalid credentials."));
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, true);

            if (signInResult.IsLockedOut)
            {
                return BadRequest(ApiResponse<object>.FailureResponse("Account is locked due to multiple failed attempts."));
            }

            if (signInResult.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Any(r => r.Equals(dto.Role, StringComparison.OrdinalIgnoreCase)))
                {
                    return Ok(ApiResponse<object>.SuccessResponse(new
                    {
                        Token = _tokenService.CreateJwt(user, dto.Role),
                        User = new { user.Email, user.UserName },
                    }, "Login successfully"));
                }
                else
                {
                    return StatusCode(403, ApiResponse<object>.FailureResponse("Your account does not have access to this section."));
                }
            }

            return Unauthorized(ApiResponse<object>.FailureResponse("Invalid password."));
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser is not null)
            {
                return Conflict(ApiResponse<object>.FailureResponse("This email already exists."));
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email.Substring(0, dto.Email.IndexOf("@")),
                FullName = dto.FullName,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                string assignedRole = string.IsNullOrEmpty(dto.CompanyName)
                                ? _roleSettings.DefaultRole
                                : _roleSettings.EmployerRole;

                var setRoleResult = await _userManager.AddToRoleAsync(user, assignedRole);

                if (setRoleResult.Succeeded)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(new
                    {
                        User = new { user.UserName, dto.FullName, dto.Email }
                    }, "Register successfully."));
                }
                else
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(setRoleResult.Errors.FirstOrDefault().Description));
                }
            }
            return BadRequest(ApiResponse<object>.FailureResponse(result.Errors.FirstOrDefault().Description));
        }

        [HttpPost("Login-Google")]
        public async Task<IActionResult> GoogleLoginAsync([FromBody] GoogleLoginRequestDto dto)
        {
            Payload payload;
            try
            {
                payload = await ValidateAsync(dto.IdToken, new ValidationSettings
                {
                    Audience = [_configuration["OAuth:Google:ClientId"]]
                });
            }
            catch (InvalidJwtException)
            {
                return BadRequest(ApiResponse<object>.FailureResponse("Google Token is invalid or expired."));
            }
            catch (Exception)
            {
                return BadRequest(ApiResponse<object>.FailureResponse("System error."));
            }

            var email = payload.Email;
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    Email = email,
                    UserName = email.Substring(0, email.IndexOf("@")),
                    FullName = payload.Name,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(ApiResponse<object>.FailureResponse("User creation failed."));

                await _userManager.AddToRoleAsync(user, dto.Role);

                return Ok(ApiResponse<object>.SuccessResponse(new
                {
                    Token = _tokenService.CreateJwt(user, dto.Role),
                    User = new { user.Email, user.UserName, dto.Role }
                }, "Login successfully"));
            }

            var roles = await _userManager.GetRolesAsync(user);

            var checkRoleResult = roles.Any(r => string.Equals(r, dto.Role, StringComparison.OrdinalIgnoreCase));

            if (!checkRoleResult)
            {
                return StatusCode(403, ApiResponse<object>.FailureResponse("Your account does not have access to this section."));
            }

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                Token = _tokenService.CreateJwt(user, dto.Role),
                User = new
                {
                    user.Email,
                    user.UserName
                }
            }, "Login successfully"));
        }
    }

}

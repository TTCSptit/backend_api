using Google.Apis.Auth;
using job.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;
using AuthResult = job.Dtos.AuthResult;

namespace job.Services
{ 
    public interface IAuthService
    {
        Task<AuthResult> ExternalLoginAsync(GoogleJsonWebSignature.Payload payload, string role);
        Task<AuthResult> RegisterAsync(RegisterDto dto);
    }
}

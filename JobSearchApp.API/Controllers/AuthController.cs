using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JobSearchApp.Core.Entities;
using JobSearchApp.Core.Enums;

namespace JobSearchApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // Здесь будет логика аутентификации
            var token = GenerateJwtToken(model.Email);
            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Здесь будет логика регистрации
            return Ok();
        }

        [HttpPost("vk")]
        public async Task<IActionResult> VkAuth([FromBody] VkAuthRequest request)
        {
            // Здесь будет логика аутентификации через VK
            return Ok();
        }

        [HttpPost("mailru")]
        public async Task<IActionResult> MailRuAuth([FromBody] MailRuAuthRequest request)
        {
            // Здесь будет логика аутентификации через Mail.ru
            return Ok();
        }

        [HttpPost("request-reset")]
        public IActionResult RequestPasswordReset([FromBody] EmailModel model)
        {
            // TODO: Implement password reset request logic (e.g., generate code, send email)
            // Этот метод, скорее всего, должен быть async, если отправляет email
            return Ok(new { Message = "Код сброса отправлен, если email существует." });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordModel model)
        {
            // TODO: Implement password reset logic (validate code, update password)
            // Этот метод может быть async, если работает с базой
            return Ok(new { Message = "Пароль успешно сброшен." });
        }
        
        [HttpPost("confirm-email")]
        public IActionResult ConfirmEmail([FromBody] CodeModel model)
        {
            // TODO: Implement email confirmation logic
            return Ok(new { Message = "Email подтвержден." });
        }
        
        [HttpPost("resend-confirmation")]
        public IActionResult ResendConfirmationEmail([FromBody] EmailModel model)
        {
            // TODO: Implement resend confirmation logic
            return Ok(new { Message = "Письмо с подтверждением отправлено повторно." });
        }

        private string GenerateJwtToken(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public class LoginModel
        {
            public string Email { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        public class RegisterModel
        {
            public string Email { get; set; } = null!;
            public string Password { get; set; } = null!;
            public string FirstName { get; set; } = null!;
            public string LastName { get; set; } = null!;
            public string? PhoneNumber { get; set; }
            public UserRole Role { get; set; }
        }

        public class CodeModel
        {
            public string Code { get; set; } = null!;
        }
        
        public class ResetPasswordModel
        {
             public string Email { get; set; } = null!;
             public string Code { get; set; } = null!;
             public string NewPassword { get; set; } = null!;
        }

        public class EmailModel
        {
            public string Email { get; set; } = null!;
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class VkAuthRequest
    {
        public string Code { get; set; }
    }

    public class MailRuAuthRequest
    {
        public string Code { get; set; }
    }
} 
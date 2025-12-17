using HabitTracker.Application.DTOs.UserDTOs;
using HabitTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto request)
        {
           
                await _authService.RegisterAsync(request);
                return Ok(new { message = "Kayıt başarılı!" });
            
          
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto request)
        {
           
                var token = await _authService.LoginAsync(request);
                return Ok(new { token = token });
            
          
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto request)
        {
            
                // Token içindeki ID'yi al
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();

                var userId = int.Parse(userIdClaim.Value);
                await _authService.UpdateUserAsync(userId, request);

                return Ok(new { message = "E-posta güncellendi." });
            
          
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();

                var userId = int.Parse(userIdClaim.Value);
                await _authService.ChangePasswordAsync(userId, request);

                return Ok(new { message = "Şifre değiştirildi." });
            
          
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> SendCode([FromBody] ForgotPasswordDto request)
        {
           
                await _authService.SendVerificationCodeAsync(request.Email);
                return Ok(new { message = "Doğrulama kodu e-postanıza gönderildi." });
            
         
        }


        // KOD DOĞRULAMA KAPISI
        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto request)
        {
            bool isValid = await _authService.VerifyCodeOnlyAsync(request.Email, request.Code);
            if (isValid)
                return Ok(new { message = "Kod doğru." });
            else
                return BadRequest(new { message = "Kod hatalı veya süresi dolmuş." });
        }

        // ŞİFRE DEĞİŞTİRME KAPISI
        [HttpPost("reset-password-confirm")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
           
                await _authService.ResetPasswordWithCodeAsync(request);
                return Ok(new { message = "Şifreniz başarıyla değiştirildi." });
          
        }


        // GOOGLE İLE GİRİŞ KAPISI
        [HttpPost("google-signin")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto request)
        {
            
                // Service katmanına token'ı gönderiyoruz, o bize JWT (kendi sistemimizin tokenı) dönüyor
                var token = await _authService.LoginWithGoogleAsync(request.Credential);
                return Ok(new { token = token });
            
          
        }

        [Authorize] // Sadece giriş yapmış kullanıcı kendini silebilir
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            
                // Token'dan ID'yi al (Başkası başkasını silemesin diye ID'yi parametre olarak almıyoruz)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();

                var userId = int.Parse(userIdClaim.Value);

                await _authService.DeleteAccountAsync(userId);

                return Ok(new { message = "Hesabınız ve tüm verileriniz başarıyla silindi." });
            
          
        }

    }
}
using HabitTracker.Application.DTOs.UserDTOs;
using HabitTracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.API.Controllers
{
    [Route("api/[controller]")] // Burası api/Auth demektir
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto request)
        {
            // View() değil, Ok() dönüyoruz. Çünkü JS bizden JSON bekliyor.
            var token = await _authService.LoginAsync(request);
            return Ok(new { token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto request)
        {
            try
            {
                // AuthService'i çağırıyoruz. Hata varsa (örn: email kayıtlıysa) catch'e düşecek.
                var user = await _authService.RegisterAsync(request);

                // Başarılı olursa 200 OK ve mesaj dönüyoruz
                return Ok(new { message = "Kayıt başarılı!", userId = user.Id });
            }
            catch (Exception ex)
            {
                // Service'den gelen "Bu e-posta kullanımda" hatasını burada yakalayıp kullanıcıya gösteriyoruz.
                return BadRequest(ex.Message);
            }
        }

    }
}

using HabitTracker.Application.DTOs.HabitDTOs;
using HabitTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitTracker.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // bu controller'a sadece token' olanlar erişebilir..
    public class HabitController : ControllerBase
    {
        private readonly IHabitService _habitService;

        public HabitController(IHabitService habitService)
        {
            _habitService = habitService;
        }

        // Token içindeki "NameIdentifier" (UserId) bilgisini okuyan yardımcı metot
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return 0;
            return int.Parse(userIdClaim.Value);
        }

        // 1. LİSTELEME
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId(); // Sadece giriş yapan kişinin verileri
            var habits = await _habitService.GetAllHabitsByUserIdAsync(userId);
            return Ok(habits);
        }

        // 2. EKLEME
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHabitDto request)
        {
            // Frontend'den UserId gelmeyebilir veya yanlış gelebilir.
            // Güvenlik için Token'dan aldığımız ID'yi basıyoruz.
            request.UserId = GetUserId();

            var result = await _habitService.CreateHabitAsync(request);
            return Ok(result);
        }

        // 3. GÜNCELLEME
        // URL Artık: https://localhost:7223/api/Habit/15
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateHabitDto request)
        {
            // Güvenlik Kontrolü: URL'deki ID ile Body'deki ID eşleşiyor mu?
            if (id != request.Id)
            {
                return BadRequest("URL'deki ID ile gönderilen verideki ID uyuşmuyor.");
            }

            
                await _habitService.UpdateHabitAsync(request);
                return Ok(new { message = "Alışkanlık güncellendi." });
            
            
        }

        // 4. SİLME
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
           
                await _habitService.DeleteHabitAsync(id);
                return Ok(new { message = "Alışkanlık silindi." });
            
          
        }

        // 5. İŞARETLEME (Tamamlandı / Geri Al)
        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleCompletion([FromBody] ToggleHabitRequestDTO request)
        {
           
                await _habitService.ToggleHabitCompletionAsync(request.HabitId, request.Date);
                return Ok(new { message = "İşlem başarılı." });
            
            
        }


        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAll()
        {
            // Token'dan UserId'yi alıyoruz
            var userId =GetUserId();

            // Servise gönderiyoruz
            await _habitService.DeleteAllHabitsByUserIdAsync(userId);

            return Ok(new { message = "Tüm alışkanlıklar temizlendi." });
        }




    }
}


using HabitTracker.Application.DTOs.BadgeDTOs;
using HabitTracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting; // IWebHostEnvironment için gerekli

namespace HabitTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")] 
    public class BadgeController : ControllerBase
    {
        private readonly IBadgeService _badgeService;
        private readonly IWebHostEnvironment _env; // Eksik olan tanım

        // Constructor (Yapıcı Metot)
        public BadgeController(IBadgeService badgeService, IWebHostEnvironment env)
        {
            _badgeService = badgeService;
            _env = env; // Burası yarım kalmıştı, düzeltildi.
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateBadgeDto request)
        {
            
            
                string dbPathString = "";

                if (request.ImageFile != null && request.ImageFile.Length > 0)
                {
                    // --- BURAYI DEĞİŞTİRİYORUZ ---

                    // 1. Kopyaladığın yolu buraya @ tırnakları içine yapıştır.
                    // ÖRNEK: @"C:\Users\bilal\source\repos\HabitTracker\Habit-Tracker Frontend\image"
                    string targetFolder = @"C:\Users\bilal\source\repos\HabitTracker\Habit-Tracker Frontend\image";

                    // Klasör yolunun doğruluğundan emin olmak için (Yoksa hata versin ki anlayalım)
                    if (!Directory.Exists(targetFolder))
                    {
                        return BadRequest($"Hedef klasör bulunamadı: {targetFolder}. Lütfen yolu kontrol edin.");
                    }

                    // 2. Benzersiz dosya ismi oluştur
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + request.ImageFile.FileName;
                    string fullFilePath = Path.Combine(targetFolder, uniqueFileName);

                    // 3. Dosyayı oraya kaydet
                    using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await request.ImageFile.CopyToAsync(fileStream);
                    }

                    // 4. Veritabanına yazılacak yol (Frontend ile aynı klasörde olduğu için)
                    dbPathString = $"image/{uniqueFileName}";
                }

                var result = await _badgeService.CreateBadgeAsync(request, dbPathString);
                return Ok(result);
            
          
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _badgeService.GetAllBadgesAsync();
            return Ok(result);
        }
        // Helper metod (Class içine ekle): Token'dan User ID okur
        private int GetUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        // --- YENİ ENDPOINT ---
        [HttpGet("earned")]
        public async Task<IActionResult> GetEarnedBadges()
        {
            int userId = GetUserId();
            var badgeIds = await _badgeService.GetEarnedBadgeIdsAsync(userId);
            return Ok(badgeIds); // Örnek çıktı: [1, 3, 5]
        }
    }
}
using HabitTracker.Application.DTOs.HabitDTOs;
using HabitTracker.Application.Interfaces;
using HabitTracker.Domain.Entities;
using HabitTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Services
{
    public class HabitService : IHabitService
    {
        private readonly IRepository<Habit> _habitRepository;
        private readonly IRepository<HabitLog> _logRepository; // Log işlemleri için bunu da çağırmalısın

        public HabitService(IRepository<Habit> habitRepository, IRepository<HabitLog> logRepository)
        {
            _habitRepository = habitRepository;
            _logRepository = logRepository;
        }

        public async Task<IEnumerable<HabitDto>> GetAllHabitsByUserIdAsync(int userId)
        {
            
                // 1. Tüm alışkanlıkları çek
                // Not: Generic Repository'de "Include" (Join) yoksa Logları ayrıca çekmen gerekebilir.
                // Şimdilik basit mantıkla gidelim:
                var allHabits = await _habitRepository.GetAllAsync();
                var userHabits = allHabits.Where(h => h.UserId == userId).ToList(); 

                // 2. Bugünün loglarını çek (Performans için Active olanlar)
                var allLogs = await _logRepository.GetAllAsync();
                var today = DateTime.Today;

                // 3. Mapping (Entity -> DTO)
                var habitDtos = userHabits.Select(h => new HabitDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Category = h.Category,
                    Frequency = h.Frequency.ToString(),
                    IsActive = h.IsActive,

                    // Hesaplanan Alan: Bugün bu alışkanlık için log var mı?
                    IsCompletedToday = allLogs.Any(l => l.HabitId == h.Id && l.CompletedDate.Date == today)
                }).ToList();

                return habitDtos;
            
        }

        public  async Task<HabitDto> CreateHabitAsync(CreateHabitDto createDto)
        {
            var habit = new Habit
            {
                Name = createDto.Name,
                Category = createDto.Category,
                Frequency = (Frequency)createDto.FrequencyId,
                UserId = createDto.UserId,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            var newHabit = await _habitRepository.CreateAsync(habit);

            return new HabitDto
            {
                Id = newHabit.Id,
                Name = newHabit.Name,
                // ... diğer alanlar
            };
        }

        public async Task DeleteHabitAsync(int habitId)
        {
            // 1. Alışkanlığı veritabanından bul
            var habit = await _habitRepository.GetByIdAsync(habitId);

            if (habit == null)
            {
                throw new Exception("Silinecek alışkanlık bulunamadı.");
            }

            // 2. Soft Delete uygula (Tamamen silmek yerine pasife çekiyoruz)
            habit.IsActive = false;

            // 3. Güncellemeyi kaydet
            await _habitRepository.UpdateAsync(habit);
        }

        

        public async Task<HabitDto> GetHabitByIdAsync(int habitId)
        {
            var habit = await _habitRepository.GetByIdAsync(habitId);
            if (habit == null)
            {
                return null;
            }
            var today = DateTime.Today;

            // 3. Log kayıtlarını kontrol et: Bu alışkanlık bugün yapılmış mı?
            // Not: Generic Repository'de "GetWhere" yoksa tüm logları çekip filtreleriz.
            var allLogs = await _logRepository.GetAllAsync();

            bool isCompleted = allLogs.Any(l => l.HabitId == habitId && l.CompletedDate.Date == today);

            // 4. Entity'yi DTO'ya çevir (Mapping)
            return new HabitDto
            {
                Id = habit.Id,
                Name = habit.Name,
                Category = habit.Category,
                Frequency = habit.Frequency.ToString(), // Enum'ı string'e çeviriyoruz
                IsActive = habit.IsActive,
                IsCompletedToday = isCompleted, // Hesapladığımız değer
                                                // CurrentStreak = ... (İleride zincir hesaplamasını buraya ekleyebilirsin)
            };
        }

        public async Task ToggleHabitCompletionAsync(int habitId, DateTime date)
        {
            // 1. Bu tarihte log var mı kontrol et
            var allLogs = await _logRepository.GetAllAsync();
            var existingLog = allLogs.FirstOrDefault(l => l.HabitId == habitId && l.CompletedDate.Date == date.Date);

            if (existingLog != null)
            {
                // VARSA SİL (İşaretlemeyi kaldır)
                await _logRepository.DeleteAsync(existingLog.Id);
            }
            else
            {
                // YOKSA EKLE (Tamamlandı işaretle)
                var newLog = new HabitLog
                {
                    HabitId = habitId,
                    CompletedDate = date,
                    CreatedDate = DateTime.Now
                };
                await _logRepository.CreateAsync(newLog);
            }
        }

        public async Task UpdateHabitAsync(UpdateHabitDto updateDto)
        {
            // 1. Güncellenecek alışkanlığı veritabanından çek
            var habit = await _habitRepository.GetByIdAsync(updateDto.Id);

            // 2. Alışkanlık var mı kontrol et
            if (habit == null)
            {
                throw new Exception("Güncellenecek alışkanlık bulunamadı.");
            }

            // 3. Yeni değerleri mevcut nesneye aktar
            habit.Name = updateDto.Name;
            habit.Category = updateDto.Category;
            habit.Frequency = (Frequency)updateDto.FrequencyId; // Int'ten Enum'a çeviriyoruz
            habit.IsActive = updateDto.IsActive; // Askıya alma veya aktif etme durumu

            // 4. Değişiklikleri veritabanına kaydet
            await _habitRepository.UpdateAsync(habit);
        }


         
    }
}

using HabitTracker.Application.DTOs.HabitDTOs;
using HabitTracker.Application.Interfaces;
using HabitTracker.Application.Interfaces.Repositories;
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
        private readonly IHabitRepository _habitRepository;
        private readonly IHabitLogRepository _logRepository; // Log işlemleri için bunu da çağırmalısın
        private readonly IUserRepository _userRepository;
        private readonly IBadgeService _badgeService;
        public HabitService(IHabitRepository habitRepository, IHabitLogRepository logRepository, IUserRepository userRepository, IBadgeService badgeService)
        {
            _habitRepository = habitRepository;
            _logRepository = logRepository;
            _userRepository = userRepository;
            _badgeService = badgeService;
        }

        public async Task<IEnumerable<HabitDto>> GetAllHabitsByUserIdAsync(int userId)
        {
            // sadece o kullanıcıya ait aktif alışkanlıkları çek
            var userHabits = await _habitRepository.GetHabitsByUserIdAsync(userId);

            // 2. ADIM: Sadece bugüne ait logları çek (Tüm logları çekmek yerine) (HIZLI)
            var today = DateTime.Today;
            var todaysLogs = await _logRepository.GetLogsByUserIdAndDateAsync(userId, today);

            // 3. Mapping
            var habitDtos = userHabits.Select(h => new HabitDto
            {
                Id = h.Id,
                Name = h.Name,
                Category = h.Category,
                Frequency = h.Frequency.ToString(),
                IsActive = h.IsActive,
                ExpirationDate = h.ExpirationDate,
                // Bellekteki küçük listeden kontrol et
                IsCompletedToday = todaysLogs.Any(l => l.HabitId == h.Id)
            }).ToList();

            return habitDtos;

        }

        public  async Task<HabitDto> CreateHabitAsync(CreateHabitDto createDto)
        {
            DateTime expirationDate;
            if ((Frequency)createDto.FrequencyId == Frequency.Weekly)
            {
                // Haftalık ise 7 gün ekle
                expirationDate = DateTime.Now.AddDays(6);
            }
            else
            {
                // Günlük ise (veya varsayılan) 1 gün ekle
                expirationDate = DateTime.Now.AddDays(0);
            }
            var habit = new Habit
            {
                Name = createDto.Name,
                Category = createDto.Category,
                Frequency = (Frequency)createDto.FrequencyId,
                UserId = createDto.UserId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                ExpirationDate = expirationDate

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

            await _habitRepository.DeleteAsync(habit.Id);
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
            // önce alışkanlığı ve kullanıcıyı bulmamız lazım
            var habit = await _habitRepository.GetByIdAsync(habitId);
            if (habit == null) throw new Exception("Alışkanlık bulunamadı");

            var user = await _userRepository.GetByIdAsync(habit.UserId);
            if (user == null) throw new Exception("kullanıcı bulunamadı");

            
            var existingLog =await _logRepository.GetLogByDateAsync(habitId,date);
            if(existingLog != null)
            {
                // --- DURUM: GERİ ALMA (UNCHECK) ---
                // 1. Logu sil
                await _logRepository.DeleteAsync(existingLog.Id);

                // 2. Puanı 1 azalt (0'ın altına düşmesin diye kontrol)
                if (user.CurrentPoints > 0)
                {
                    user.CurrentPoints--;
                }
            }

            else
            {
                // --- DURUM: TAMAMLAMA (CHECK) ---
                // 1. Logu ekle
                var newLog = new HabitLog
                {
                    HabitId = habitId,
                    CompletedDate = date,
                    CreatedDate = DateTime.Now
                };
                await _logRepository.CreateAsync(newLog);

                // 2. Puanı 1 arttır
                user.CurrentPoints++;
            }
            // C. SEVİYE HESAPLAMA (ORTAK MANTIK)
            // Kural: Her 10 puanda 1 level.
            // Örnek: 0-9 Puan -> Level 1
            //       10-19 Puan -> Level 2
            //       55 Puan -> Level 6 ((55 / 10) + 1)

            user.Level = (user.CurrentPoints / 10) + 1;

            // D. Kullanıcıyı Güncelle (Puan ve Level değişti)
            await _userRepository.UpdateAsync(user);
            if (existingLog == null) // Yani yeni log eklendiyse (Tamamlandıysa)
            {
                await _badgeService.CheckAndAwardBadgesAsync(user.Id);
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
            habit.IsActive = updateDto.IsActive; // Askıya alma veya aktif etme durumu

            DateTime expirationDate;

            if (habit.Frequency != (Frequency)updateDto.FrequencyId)
            {
                habit.Frequency = (Frequency)updateDto.FrequencyId; // Int'ten Enum'a çeviriyoruz

                if (habit.Frequency == Frequency.Weekly)
                {
                    // Haftalığa çevirdi , şu anki tarihe 6 gün ekledi (ExpirationDate şu anki güne 6 gün eklenmiş olarak ayarlandı)
                    habit.ExpirationDate = DateTime.Now.AddDays(6);
                }
                else
                {
                    // günlüğe çevirdi , şu anki tarihe gün eklemedi (ExpirationDate şu anki gün sonuna kadar ayarladı)
                    habit.ExpirationDate = DateTime.Now.AddDays(0);
                }
            }
                
            // 4. Değişiklikleri veritabanına kaydet
            await _habitRepository.UpdateAsync(habit);
        }


         
    }
}

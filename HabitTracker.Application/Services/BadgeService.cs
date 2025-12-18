using HabitTracker.Application.DTOs.BadgeDTOs;
using HabitTracker.Application.Interfaces;
using HabitTracker.Application.Interfaces.Repositories;
using HabitTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using HabitTracker.Domain.Constants; // Gerekli kütüphane

namespace HabitTracker.Application.Services
{
    public class BadgeService : IBadgeService
    {
        private readonly IBadgeRepository _badgeRepository;
        private readonly IRepository<UserBadge> _userBadgeRepository; // Kullanıcı-Rozet tablosu
        private readonly IHabitLogRepository _logRepository; // Logları kontrol etmek için
        private readonly IRepository<User> _userRepository; // Puanı kontrol etmek için/ Dosya yolunu bulmak için gerekli
        private readonly IHabitRepository _habitRepository;


        public BadgeService(IBadgeRepository badgeRepository,IRepository<UserBadge> userBadgeRepository,IHabitLogRepository logRepository,IRepository<User> userRepository,
            IHabitRepository habitRepository)
        {
            _badgeRepository = badgeRepository;
            _userBadgeRepository = userBadgeRepository;
            _logRepository = logRepository;
            _habitRepository = habitRepository;
            _userRepository = userRepository;

        }

        public async Task CheckAndAwardBadgesAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            // Kullanıcının ZATEN sahip olduğu rozetleri geitr.
            var earnedBadgeIds = await _badgeRepository.GetUserEarnedBadgeIdsAsync(userId);
       
            // Tüm logları çek (Analiz için)
            // Not: Performans için sadece son 60 günü çekmek daha iyi olabilir ama şimdilik tümünü alalım.
            var userHabits = await _habitRepository.GetHabitsByUserIdAsync(userId);
            var userHabitIds= userHabits.Select(h=>h.Id).ToList();

            var allLogs= await _logRepository.GetAllAsync();

            // Sadece bu kullanıcının alışkanlıklarına ait logları filtreliyoruz
            // (Habit nesnesine girmeden HabitId üzerinden kontrol ediyoruz)
            var myLogs = allLogs
            .Where(l => userHabitIds.Contains(l.HabitId))
            .OrderByDescending(l => l.CompletedDate)
            .ToList();

            // --- KURAL 0: İLK ADIM ROZETİ ---
            await CheckAndGiveBadge(userId, earnedBadgeIds, BadgeNames.FirstStep, () =>
            {
                // Eğer kullanıcının herhangi bir logu varsa (Count > 0) rozeti ver.
                return myLogs.Any();
            });

            // --- KURAL 1: İSTİKRAR ROZETİ (30 Puan) ---
            await CheckAndGiveBadge(userId, earnedBadgeIds, BadgeNames.Stability, () =>
            {
                return user.CurrentPoints >= 30;
            });

            // --- KURAL 2: AYLIK ŞAMPİYON (30 Gün Aralıksız Seri) ---
            await CheckAndGiveBadge(userId, earnedBadgeIds, BadgeNames.MonthlyChampion, () =>
            {
                // Logların tarihlerini (saatsiz) tekilleştir
                var dates = myLogs.Select(l => l.CompletedDate.Date).Distinct().OrderByDescending(d => d).ToList();
                return CalculateStreak(dates) >= 30;
            });

            // --- KURAL 3: ERKEN KALKAN (14 Gün, 05:00-09:00 Arası) ---
            await CheckAndGiveBadge(userId, earnedBadgeIds, BadgeNames.EarlyBird, () =>
            {
                // Sabah 5 ile 9 arasında tamamlanmış logların tarihlerini al
                var earlyDates = myLogs
                    .Where(l => l.CompletedDate.Hour >= 5 && l.CompletedDate.Hour < 9)
                    .Select(l => l.CompletedDate.Date)
                    .Distinct()
                    .Count();

                // 14 farklı günde sabah yapılmış mı?
                return earlyDates >= 14;
            });
        }

        //YArdımcı metot
        // Rozet verme işlemini standartlaştıran metot
        private async Task CheckAndGiveBadge(int userId, IEnumerable<int> earnedIds, string badgeName, Func<bool> condition)
        {
            // 1. Önce tüm rozetleri çekip ismine göre bulalım (Trim ile boşlukları temizleyelim)
            var allBadges = await _badgeRepository.GetAllAsync();

            var badge = allBadges.FirstOrDefault(b => b.Name.Trim().ToLower() == badgeName.Trim().ToLower());

            if (badge == null)
            {
                // Loglama yapabilirsin: Console.WriteLine($"HATA: '{badgeName}' isimli rozet veritabanında bulunamadı!");
                return;
            }

            // 2. Kullanıcı zaten sahip mi?
            if (earnedIds.Contains(badge.Id)) return;

            // 3. Şart sağlanıyor mu?
            if (condition())
            {
                var newBadge = new UserBadge
                {
                    UserId = userId,
                    BadgeId = badge.Id,
                    EarnedDate = DateTime.Now,
                    CreatedDate = DateTime.Now
                };
                await _userBadgeRepository.CreateAsync(newBadge);
            }
        }

        // Ardışık gün sayısını (Streak) hesaplayan algoritma
        private int CalculateStreak(List<DateTime> dates)
        {
            if (dates.Count == 0) return 0;

            int streak = 1; // Bugün (veya son yapılan gün) sayılır
            for (int i = 0; i < dates.Count - 1; i++)
            {
                // Eğer bir sonraki tarih, şimdikinden tam 1 gün önceyse seri devam ediyordur.
                if ((dates[i] - dates[i + 1]).TotalDays == 1)
                {
                    streak++;
                }
                else
                {
                    break; // Seri bozuldu
                }
            }
            return streak;
        }
        public async Task<BadgeDTO> CreateBadgeAsync(CreateBadgeDto createDto, string imagePath)
        {
            var badge = new Badge
            {
                Name = createDto.Name,
                Description = createDto.Description,
                ImageUrl = imagePath, // Controller'dan gelen yolu buraya atıyoruz
                CreatedDate = DateTime.Now
            };

            var createdBadge = await _badgeRepository.CreateAsync(badge);

            return new BadgeDTO
            {
                Id = createdBadge.Id,
                Name = createdBadge.Name,
                Description = createdBadge.Description,
                ImageUrl = createdBadge.ImageUrl
            };
        }

        public async Task<IEnumerable<BadgeDTO>> GetAllBadgesAsync()
        {
            // ESKİ KOD (Hata veren yer muhtemelen burasıydı):
            // throw new NotImplementedException(); 

            // YENİ KOD (Veritabanından çekip DTO'ya çeviriyoruz):
            var badges = await _badgeRepository.GetAllAsync();

            return badges.Select(b => new BadgeDTO
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                ImageUrl = b.ImageUrl // Resim yolunu da ön yüze gönderiyoruz
            }).OrderByDescending(o=>o.Id).ToList();
        }

        public async Task<IEnumerable<int>> GetEarnedBadgeIdsAsync(int userId)
        {
            return await _badgeRepository.GetUserEarnedBadgeIdsAsync(userId);
        }
    }
}


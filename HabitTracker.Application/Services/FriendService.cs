using HabitTracker.Application.DTOs.FriendshipDTOs;
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
    public class FriendService : IFriendService
    {
        // Not: Burada IRepository pattern kullanıyorsan onu inject et. 
        // Örnek anlaşılır olsun diye doğrudan DbContext veya Repo mantığıyla yazıyorum.
        private readonly IFriendshipRepository _friendshipRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMailService _mailService; // SENİN SERVİSİN

        public FriendService(IFriendshipRepository friendshipRepo, IUserRepository userRepo, IMailService mailService)
        {
            _friendshipRepo = friendshipRepo;
            _userRepo = userRepo;
            _mailService = mailService;
        }

        public async Task SendRequestAsync(int currentUserId, string targetEmail)
        {
            // 1. Hedef kullanıcıyı bul Sadece hedef kullanıcıyı çek (Tüm tabloyu değil)
            var targetUser = await _userRepo.GetByEmailAsync(targetEmail);

            if (targetUser == null) throw new Exception("Kullanıcı bulunamadı.");
            if (targetUser.Id == currentUserId) throw new Exception("Kendine istek atamazsın.");

            var currentUser = await _userRepo.GetByIdAsync(currentUserId);

            // 2. OPTİMİZASYON: Veri çekmeden sadece Var mı/Yok mu kontrolü yap
            // Veritabanından 0 veri transferi, sadece true/false sonucu gelir.
            bool alreadyExists = await _friendshipRepo.HasFriendshipOrRequestAsync(currentUserId, targetUser.Id);

            if (alreadyExists) throw new Exception("Zaten bir istek veya arkadaşlık mevcut.");

            // 3. İsteği oluştur
            var friendship = new Friendship
            {
                RequesterId = currentUserId,
                AddresseeId = targetUser.Id,
                Status = FriendRequestStatus.Pending,
                RequestDate = DateTime.Now
            };

            await _friendshipRepo.CreateAsync(friendship);

            // 4. MAİL GÖNDERME İŞLEMİ (GÜNCELLENMİŞ - HIZLI)
            string subject = "Yeni Arkadaşlık İsteği! 👋";
            string body = $@"
                <h3>Merhaba {targetUser.Name},</h3>
                <p><strong>{currentUser.Name}</strong> ({currentUser.Email}) sana HabitQuest üzerinden arkadaşlık isteği gönderdi.</p>
                <p>Cevaplamak için uygulamaya giriş yapabilirsin.</p>
                <br/>
                <p>İyi alışkanlıklar dileriz!</p>
            ";

            await _mailService.SendEmailAsync(targetUser.Email, subject, body);
        }

        public async Task RespondToRequestAsync(int currentUserId, int requestId, bool isAccepted)
        {
            // 1. İsteği detaylarıyla çek (Requester ve Addressee bilgileriyle)
            var friendship = await _friendshipRepo.GetFriendshipWithDetailsAsync(requestId);

            if (friendship == null) throw new Exception("İstek bulunamadı.");

            // Güvenlik Kontrolü: İsteği cevaplayan kişi, gerçekten hedefteki kişi mi?
            if (friendship.AddresseeId != currentUserId) throw new Exception("Bu isteği cevaplama yetkiniz yok.");

            // 2. Durumu Güncelle (Accepted veya Rejected)
            friendship.Status = isAccepted ? FriendRequestStatus.Accepted : FriendRequestStatus.Rejected;
            await _friendshipRepo.UpdateAsync(friendship);

            // 3. MAİL GÖNDERME İŞLEMİ (Her iki durum için)
            // Eğer kullanıcı bilgileri veritabanından başarıyla geldiyse
            if (friendship.Requester != null && friendship.Addressee != null)
            {
                string subject = "";
                string body = "";

                if (isAccepted)
                {
                    // --- KABUL EDİLME DURUMU ---
                    subject = "Arkadaşlık İsteğin Kabul Edildi! 🎉";
                    body = $@"
                <h3>Harika Haber {friendship.Requester.Name},</h3>
                <p><strong>{friendship.Addressee.Name}</strong> gönderdiğin arkadaşlık isteğini kabul etti.</p>
                <p>Artık beraber alışkanlık yapabilirsiniz.</p>
            ";
                }
                else
                {
                    // --- REDDEDİLME DURUMU ---
                    subject = "Arkadaşlık İsteğin Reddedildi 😔";
                    body = $@"
                <h3>Merhaba {friendship.Requester.Name},</h3>
                <p>Maalesef <strong>{friendship.Addressee.Name}</strong> gönderdiğin arkadaşlık isteğini kabul etmedi.</p>
                <p>Merak etme, HabitQuest'te yeni arkadaşlar bulabilir ve alışkanlıklarını geliştirmeye devam edebilirsin!</p>
            ";
                }

                // Ortak İmza
                body += @"
            <br/>
            <p>İyi alışkanlıklar dileriz!</p>
            <p><em>HabitQuest Ekibi</em></p>
        ";

                // Mail Gönderme (Hata olursa akış bozulmasın diye Try-Catch)
                
                await _mailService.SendEmailAsync(friendship.Requester.Email, subject, body);
                
              
            }
        }

        public async Task<IEnumerable<FriendRequestListDto>> GetReceivedRequestsAsync(int userId)
        {
            var pendingRequests = await _friendshipRepo.GetPendingRequestsForUserAsync(userId);
            // Sadece DTO'ya çevirme işlemi (Mapping) burada kalır
            return pendingRequests.Select(f => new FriendRequestListDto
            {
                RequestId = f.Id,
                FriendName = f.Requester?.Name ?? "İsimsiz", // Artık dolu gelir!
                FriendEmail = f.Requester?.Email ?? "Gizli",
                Status = f.Status.ToString(),
                RequestDate = f.RequestDate
            }).ToList();

        }

        public async Task<IEnumerable<FriendRequestListDto>> GetSentRequestsAsync(int userId)
        {
            // YENİ OPTİMİZE KOD:
            // Repository bizim için Addressee (Hedef Kişi) verisini doldurup getirdi.
            var sentRequests = await _friendshipRepo.GetSentRequestsForUserAsync(userId);

            return sentRequests.Select(f => new FriendRequestListDto
            {
                RequestId = f.Id,
                // Addressee: İsteği gönderdiğim kişi
                FriendName = f.Addressee?.Name ?? "Bilinmeyen Kullanıcı",
                FriendEmail = f.Addressee?.Email ?? "Bilinmiyor",
                Status = f.Status.ToString(), // Bekliyor, Kabul Edildi vs.
                RequestDate = f.RequestDate
            }).ToList();

        }

        public async Task<IEnumerable<FriendRequestListDto>> GetActiveFriendsAsync(int userId)
        {
            var friendships = await _friendshipRepo.GetAcceptedFriendshipsAsync(userId);

            return friendships.Select(f =>
            {
                // Eğer isteği ben gönderdiysem (Requester == Ben), Arkadaşım Addressee'dir.
                // Eğer bana geldiyse (Addressee == Ben), Arkadaşım Requester'dır.
                var friendUser = f.RequesterId == userId ? f.Addressee : f.Requester;

                return new FriendRequestListDto
                {
                    RequestId = f.Id,
                    FriendId = friendUser.Id, // <--- BURAYI DOLDURUYORUZ
                    FriendName = friendUser?.Name ?? "Bilinmiyor",
                    FriendEmail = friendUser?.Email ?? "Gizli",
                    Status = "Arkadaş",
                    RequestDate = f.RequestDate
                };
            }).ToList();
        }

        public async Task RemoveFriendshipAsync(int currentUserId, int friendshipId)
        {
            var friendship = await _friendshipRepo.GetByIdAsync(friendshipId);

            if (friendship == null)
                throw new Exception("Arkadaşlık kaydı bulunamadı.");

            // GÜVENLİK KONTROLÜ: 
            // Sadece arkadaşlığın tarafları (İsteği atan veya kabul eden) silebilir.
            if (friendship.RequesterId != currentUserId && friendship.AddresseeId != currentUserId)
            {
                throw new Exception("Bu arkadaşlığı silme yetkiniz yok.");
            }

            // İlişkili kayıt temizliği (AdminService'de yaptığımız gibi) gerekebilir 
            // ama normal kullanıcı sildiğinde sadece Friendship silinsin diyorsan:
            await _friendshipRepo.DeleteAsync(friendshipId);
        }
    }
}

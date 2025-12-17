using HabitTracker.Application.DTOs.HabitShareDTOs;
using HabitTracker.Application.Interfaces;
using HabitTracker.Application.Interfaces.Repositories;
using HabitTracker.Domain.Entities;
using HabitTracker.Domain.Enums;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Services
{
    public class HabitShareService : IHabitShareService
    {
        private readonly IHabitInvitationRepository _invitationRepo;
        private readonly IHabitRepository _habitRepo; // Alışkanlığı kopyalamak için
        private readonly IUserRepository _userRepo;   // İsimleri ve mailleri almak için
        private readonly IMailService _mailService;   // Mail atmak için

        // FriendshipRepository'yi de ekleyebilirsin, arkadaş olup olmadıklarını kontrol etmek için.

        public HabitShareService(
            IHabitInvitationRepository invitationRepo,
            IHabitRepository habitRepo,
            IUserRepository userRepo,
            IMailService mailService)
        {
            _invitationRepo = invitationRepo;
            _habitRepo = habitRepo;
            _userRepo = userRepo;
            _mailService = mailService;
        }

        public async Task SendInvitationAsync(int senderId, SendHabitInvitationDto dto)
        {
            // Gerekli verileri çek
            var sender = await _userRepo.GetByIdAsync(senderId);
            var friend = await _userRepo.GetByIdAsync(dto.FriendId);
            var habit = await _habitRepo.GetByIdAsync(dto.HabitId);

            if (habit == null) throw new Exception("Alışkanlık bulunamadı.");
            // Burada basitçe arkadaşlık kontrolü de yapılabilir ama şimdilik pas geçiyorum.

            // Daveti oluştur
            var invitation = new HabitInvitation
            {
                SenderId = senderId,
                ReceiverId = dto.FriendId,
                HabitId = dto.HabitId,
                Status = FriendRequestStatus.Pending,
                RequestDate = DateTime.Now
            };

            await _invitationRepo.CreateAsync(invitation);

            // MAİL GÖNDERME
            string subject = $"HabitQuest: {sender.Name} seninle bir alışkanlık paylaşmak istiyor!";
            string body = $@"
            <h3>Merhaba {friend.Name},</h3>
            <p>Arkadaşın <strong>{sender.Name}</strong>, <em>'{habit.Name}'</em> adlı alışkanlığını seninle paylaşmak istiyor.</p>
            <p>Kabul edersen bu alışkanlık senin listene de eklenecek.</p>
            <br/>
            <p>Uygulamadaki 'İstekler' menüsünden onaylayabilirsin.</p>
        ";

            await _mailService.SendEmailAsync(friend.Email, subject, body);

        }

        public async Task RespondToInvitationAsync(int receiverId, RespondHabitInvitationDto dto)
        {
          
                // 1. Daveti bul
                var invitation = await _invitationRepo.GetByIdAsync(dto.InvitationId);

                if (invitation == null)
                    throw new Exception("Davet bulunamadı.");

                if (invitation.ReceiverId != receiverId)
                    throw new Exception("Bu işlemi yapmaya yetkiniz yok.");

                if (invitation.Status != FriendRequestStatus.Pending)
                    throw new Exception("Bu davet daha önce zaten cevaplanmış.");

                // 2. İlgili kişileri ve alışkanlığı mail/kopyalama işlemi için çek
                var sender = await _userRepo.GetByIdAsync(invitation.SenderId);    // Daveti gönderen (Mail alacak kişi)
                var receiver = await _userRepo.GetByIdAsync(invitation.ReceiverId); // Cevap veren (Şu anki kullanıcı)
                var originalHabit = await _habitRepo.GetByIdAsync(invitation.HabitId);

                if (sender == null || receiver == null || originalHabit == null)
                    throw new Exception("İlgili kayıtlar (Kullanıcı veya Alışkanlık) bulunamadı.");

                // 3. Davet Durumunu Güncelle
                invitation.Status = dto.IsAccepted ? FriendRequestStatus.Accepted : FriendRequestStatus.Rejected;
                await _invitationRepo.UpdateAsync(invitation);

                // 4. EĞER KABUL EDİLDİYSE -> Alışkanlığı Kopyala
                if (dto.IsAccepted)
                {
                    var newHabit = new Habit
                    {
                        UserId = receiverId,
                        Name = originalHabit.Name,
                        Category = originalHabit.Category,
                        Frequency = originalHabit.Frequency,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        ExpirationDate = DateTime.Now.AddDays(originalHabit.Frequency == Frequency.Weekly ? 7 : 1)
                    };
                    await _habitRepo.CreateAsync(newHabit);
                }

                // 5. MAİL GÖNDERME İŞLEMİ (Geri Bildirim)
               
                    string subject = "";
                    string body = "";

                    if (dto.IsAccepted)
                    {
                        // --- KABUL EDİLDİ MAİLİ ---
                        subject = "Alışkanlık Davetin Kabul Edildi! 🎯";
                        body = $@"
                <h3>Harika Haber {sender.Name},</h3>
                <p><strong>{receiver.Name}</strong>, gönderdiğin <strong>'{originalHabit.Name}'</strong> alışkanlığı davetini kabul etti.</p>
                <p>Alışkanlık artık onun listesine de eklendi. Birlikte zinciri kırmadan devam edin!</p>";
                    }
                    else
                    {
                        // --- REDDEDİLDİ MAİLİ ---
                        subject = "Alışkanlık Daveti Hakkında 📩";
                        body = $@"
                <h3>Merhaba {sender.Name},</h3>
                <p><strong>{receiver.Name}</strong>, gönderdiğin <strong>'{originalHabit.Name}'</strong> alışkanlığı davetini maalesef şu an için reddetti.</p>
                <p>Başka arkadaşlarınla yeni alışkanlıklar paylaşmaya devam edebilirsin!</p>";
                    }

                    body += "<br/><p>Keyifli takipleri dileriz!<br/><em>HabitQuest Ekibi</em></p>";

                    // Daveti gönderen kişiye (sender.Email) durumu bildir
                    await _mailService.SendEmailAsync(sender.Email, subject, body);
                
              
            
      }




        public async Task<IEnumerable<HabitInvitationListDto>> GetPendingInvitationsAsync(int userId)
        {
                var list = await _invitationRepo.GetPendingInvitationsAsync(userId);
                return list.Select(x => new HabitInvitationListDto
                {
                    InvitationId = x.Id,
                    SenderName = x.Sender.Name,
                    HabitName = x.Habit.Name,
                    Category = x.Habit.Category
                }).ToList();

            }

      
       
    }
}

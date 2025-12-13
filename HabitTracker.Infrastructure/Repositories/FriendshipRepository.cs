using HabitTracker.Application.Interfaces.Repositories;
using HabitTracker.Domain.Entities;
using HabitTracker.Domain.Enums;
using HabitTracker.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Infrastructure.Repositories
{
    public class FriendshipRepository : Repository<Friendship>, IFriendshipRepository
    {
        private readonly HabitTrackerDbContext _context;

        public FriendshipRepository(HabitTrackerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Friendship>> GetAcceptedFriendshipsAsync(int userId)
        {
            return await _context.Friendships
        .Include(f => f.Requester) // Sen olabilirsin
        .Include(f => f.Addressee) // Arkadaşın olabilir
        .Where(f => (f.RequesterId == userId || f.AddresseeId == userId)
                    && f.Status == FriendRequestStatus.Accepted)
        .ToListAsync();
        }

        public async Task<Friendship> GetFriendshipWithDetailsAsync(int friendshipId)
        {
            return await _context.Friendships
        .Include(f => f.Requester) // İsteği gönderen (Mail atacağımız kişi)
        .Include(f => f.Addressee) // İsteği kabul eden (İsmi lazım olacak)
        .FirstOrDefaultAsync(f => f.Id == friendshipId);

        }

        public async Task<IEnumerable<Friendship>> GetPendingRequestsForUserAsync(int userId)
        {
            // İŞTE PERFORMANS BURADA:
            // 1. Include ile Gönderen bilgisini al.
            // 2. Where ile SADECE ilgili kayıtları filtrele (SQL'de WHERE çalışır).
            // 3. ToListAsync ile veriyi çek.
            
            return await _context.Friendships
                .Include(f => f.Requester) // İsimleri çekmek için şart!
                .Where(f => f.AddresseeId == userId && f.Status == FriendRequestStatus.Pending)
                .ToListAsync();
        }

        public async Task<IEnumerable<Friendship>> GetSentRequestsForUserAsync(int userId)
        {
            return await _context.Friendships
            .Include(f => f.Addressee) // Hedef kullanıcının bilgilerini doldur
            .Where(f => f.RequesterId == userId) // Gönderen benim
            .ToListAsync();
        }

        public async Task<bool> HasFriendshipOrRequestAsync(int userId1, int userId2)
        {
            // SQL: SELECT CASE WHEN EXISTS (...) THEN 1 ELSE 0 END
            // Veri çekmez, sadece Var/Yok (bool) döner. Çok hızlıdır.
            return await _context.Friendships.AnyAsync(f =>
                (f.RequesterId == userId1 && f.AddresseeId == userId2) ||
                (f.RequesterId == userId2 && f.AddresseeId == userId1));
        }
    }
}

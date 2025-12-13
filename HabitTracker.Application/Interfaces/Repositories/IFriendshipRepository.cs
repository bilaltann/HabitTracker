using HabitTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Interfaces.Repositories
{
    public interface IFriendshipRepository:IRepository<Friendship>
    {
        // Kendimize özel metodumuz:
        // Hem filtreleme yapacak hem de Requester bilgisini (Include) getirecek.
        Task<IEnumerable<Friendship>> GetPendingRequestsForUserAsync(int userId);

        //Arkadaşlık Kontrolü İçin Özel Sorgu
        Task<bool> HasFriendshipOrRequestAsync(int userId1, int userId2);

        Task<IEnumerable<Friendship>> GetSentRequestsForUserAsync(int userId);

        // Tek bir isteği, gönderen ve alan kullanıcı detaylarıyla getirir
        Task<Friendship> GetFriendshipWithDetailsAsync(int friendshipId);
        Task<IEnumerable<Friendship>> GetAcceptedFriendshipsAsync(int userId);



    }
}

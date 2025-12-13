using HabitTracker.Application.DTOs.FriendshipDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Interfaces
{
    public interface IFriendService
    {
        Task SendRequestAsync(int currentUserId, string targetEmail);
        Task RespondToRequestAsync(int currentUserId, int requestId, bool isAccepted);
        // Gelen istekleri listeleme metodu da eklenebilir...
        Task<IEnumerable<FriendRequestListDto>> GetReceivedRequestsAsync(int userId);
        Task<IEnumerable<FriendRequestListDto>> GetSentRequestsAsync(int userId);

        Task<IEnumerable<FriendRequestListDto>> GetActiveFriendsAsync(int userId);

    }
}

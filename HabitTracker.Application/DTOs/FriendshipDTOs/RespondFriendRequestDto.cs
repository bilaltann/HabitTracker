using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.FriendshipDTOs
{
    public class RespondFriendRequestDto
    {
        public int RequestId { get; set; }
        public bool IsAccepted { get; set; } // True: Kabul, False: Red
    }
}

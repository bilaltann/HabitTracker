using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.HabitShareDTOs
{
    public class SendHabitInvitationDto
    {
        public int HabitId { get; set; }
        public int FriendId { get; set; } // Hedef arkadaşın ID'si
    }
}

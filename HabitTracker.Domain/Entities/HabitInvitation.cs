using HabitTracker.Domain.Common;
using HabitTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Domain.Entities
{
    public class HabitInvitation : BaseEntity
    {
        public int SenderId { get; set; } // Daveti gönderen
        public User Sender { get; set; }

        public int ReceiverId { get; set; } // Daveti alan (Arkadaş)
        public User Receiver { get; set; }

        public int HabitId { get; set; } // Hangi alışkanlık paylaşılıyor?
        public Habit Habit { get; set; }

        public FriendRequestStatus Status { get; set; } // Pending, Accepted, Rejected
        public DateTime RequestDate { get; set; } = DateTime.Now;
    }
}

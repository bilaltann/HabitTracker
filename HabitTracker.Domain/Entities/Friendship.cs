using HabitTracker.Domain.Common;
using HabitTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Domain.Entities
{
    public class Friendship:BaseEntity
    {
        public int RequesterId { get; set; }
        public User Requester { get; set; }

        // İsteği Alan
        public int AddresseeId { get; set; }
        public User Addressee { get; set; }

        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
        public DateTime RequestDate { get; set; } = DateTime.Now;
    }
}

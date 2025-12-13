using HabitTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public byte[] PasswordHash { get; set; } // Şifreyi asla plain-text (string) tutmayın!
        public byte[] PasswordSalt { get; set; }

        public int CurrentPoints { get;  set; } // Dışarıdan set edilemesin
        public int Level { get;  set; }

        // Navigation Properties
        public ICollection<Habit> Habits { get; set; }
        public ICollection<UserBadge> UserBadges { get; set; } // Çoka-çok ilişki
        public ICollection<HabitLog> HabitLogs { get; set; }
        // Gönderdiği istekler
        public ICollection<Friendship> SentFriendRequests { get; set; }

        // Aldığı istekler
        public ICollection<Friendship> ReceivedFriendRequests { get; set; }
    }
}

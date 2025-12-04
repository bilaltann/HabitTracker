using HabitTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Domain.Entities
{
    public class UserBadge:BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int BadgeId { get; set; }
        public Badge Badge { get; set; }

        public DateTime EarnedDate { get; set; }
    }
}

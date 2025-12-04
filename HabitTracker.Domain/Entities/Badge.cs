using HabitTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Domain.Entities
{
    public class Badge:BaseEntity
    {
        public string Name { get; set; } // "Maratoncu"
        public string Description { get; set; }
        public ICollection<UserBadge> UserBadges { get; set; }
        public string ImageUrl { get; set; } 
    }
}

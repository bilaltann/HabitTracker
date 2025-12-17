using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.AdminDTOs
{
    public class AdminFriendshipListDto
    {
        public int Id { get; set; }
        public string RequesterName { get; set; } // İsteyen
        public string AddresseeName { get; set; } // Hedef
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

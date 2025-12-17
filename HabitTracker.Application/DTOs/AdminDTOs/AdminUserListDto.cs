using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.AdminDTOs
{
    public class AdminUserListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int Level { get; set; }
        public int CurrentPoints { get; set; }

        public string Role { get; set; }
    }
}

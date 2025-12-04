using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.UserDTOs
{
    public class UserRegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; } // Kullanıcı adı isteğe bağlı olabilir ama gereksinimde var.
    }
}


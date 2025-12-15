using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.UserDTOs
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string Code { get; set; } // Güvenlik için kodu tekrar isteyeceğiz
        public string NewPassword { get; set; }
    }
}
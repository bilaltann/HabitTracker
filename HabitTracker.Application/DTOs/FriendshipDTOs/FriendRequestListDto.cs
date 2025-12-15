using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs.FriendshipDTOs
{
    public class FriendRequestListDto
    {
        public int RequestId { get; set; }
        public string FriendName { get; set; } // İsteği atan veya atılan kişinin adı
        public int FriendId { get; set; }  // <--- BUNU EKLE (Kullanıcı ID'si)

        public string FriendEmail { get; set; }
        public string Status { get; set; }
        public DateTime RequestDate { get; set; }
    }
}

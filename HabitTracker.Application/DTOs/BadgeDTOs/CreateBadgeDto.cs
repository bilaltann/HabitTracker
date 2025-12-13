using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HabitTracker.Application.DTOs.BadgeDTOs
{
    public class CreateBadgeDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        
        // String Url yerine Dosya nesnesi alıyoruz
        public IFormFile ImageFile { get; set; }
    }
}

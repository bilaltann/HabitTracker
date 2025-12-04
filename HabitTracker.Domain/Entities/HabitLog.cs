using HabitTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Domain.Entities
{
    public class HabitLog:BaseEntity
    {
        // Hangi alışkanlık yapıldı?
        public int HabitId { get; set; }
        public Habit Habit { get; set; }

        public DateTime CompletedDate { get; set; }

        // Opsiyonel: Eğer alışkanlık "Kitap Oku" ise ve hedef "20 sayfa" ise,
        // o gün kaç sayfa okuduğunu buraya yazabilir.
        public int? Quantity { get; set; }
        // Frontend'in "Butonu yeşil mi yapayım gri mi?" diye bilmesi için:
        public bool IsCompletedToday { get; set; }
    }
}

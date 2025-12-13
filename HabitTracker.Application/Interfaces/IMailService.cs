using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Interfaces
{
    public interface IMailService
    {
        Task SendEmailAsync(string tomail, string subject, string body);
    }
}

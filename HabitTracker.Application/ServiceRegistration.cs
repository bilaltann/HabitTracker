using HabitTracker.Application.Interfaces;
using HabitTracker.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {

            // --- YENİ EKLEDİĞİMİZ SATIR BURASI: ---
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IHabitService, HabitService>();
            services.AddScoped<IFriendService, FriendService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IBadgeService, BadgeService>();
            services.AddScoped<IHabitShareService, HabitShareService>();
            services.AddScoped<IAdminService, AdminService>();


        }
    }
}

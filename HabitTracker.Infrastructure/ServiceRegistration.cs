using HabitTracker.Application.Interfaces;
using HabitTracker.Application.Interfaces.Repositories;
using HabitTracker.Application.Services;
using HabitTracker.Domain.Entities;
using HabitTracker.Infrastructure.Contexts;
using HabitTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Infrastructure
{
    public static class ServiceRegistration
    {
        // IConfiguration parametresini buraya ekliyoruz ki Web katmanından ayarları alabilelim.
        public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<HabitTrackerDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            // "DefaultConnection" ismi appsettings.json ile AYNI olmalı.

            // Repository kayıtları...
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IHabitRepository, HabitRepository>();
            services.AddScoped<IHabitLogRepository, HabitLogRepository>();
            services.AddScoped<IBadgeRepository, BadgeRepository>();
            services.AddScoped<IRepository<UserBadge>, Repository<UserBadge>>();

        }
    }
}

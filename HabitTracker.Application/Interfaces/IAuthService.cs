using HabitTracker.Application.DTOs.UserDTOs;
using HabitTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Interfaces
{
    public interface IAuthService
    {
        // Kayıt işlemi: Başarılı olursa User döner, olmazsa null veya hata fırlatır.
        Task<User> RegisterAsync(UserRegisterDto registerDto);

        // Giriş işlemi: Başarılı olursa JWT Token (string) döner.
        Task<string> LoginAsync(UserLoginDto loginDto);

    }
}

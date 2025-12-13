using HabitTracker.Application.DTOs.UserDTOs;
using HabitTracker.Application.Interfaces;
using HabitTracker.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HabitTracker.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IRepository<User> userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        // --- 1. KAYIT OLMA ---
        public async Task<User> RegisterAsync(UserRegisterDto registerDto)
        {
            var users = await _userRepository.GetAllAsync();
            if (users.Any(u => u.Email == registerDto.Email))
            {
                throw new Exception("Bu e-posta adresi zaten kullanımda.");
            }

            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = registerDto.Email,
                Name = registerDto.Name,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CurrentPoints = 0,
                Level = 1
            };

            return await _userRepository.CreateAsync(user);
        }

        // --- 2. GİRİŞ YAPMA ---
        public async Task<string> LoginAsync(UserLoginDto loginDto)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == loginDto.Email);

            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new Exception("Şifre hatalı.");
            }

            return CreateToken(user);
        }

        // --- 3. PROFİL (E-POSTA) GÜNCELLEME ---
        public async Task UpdateUserAsync(int userId, UserUpdateDto updateDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            // Eğer e-posta aynıysa hiçbir şey yapma (Veritabanını yorma)
            if (updateDto.Email == user.Email) return;

            // Başkası kullanıyor mu?
            var allUsers = await _userRepository.GetAllAsync();
            if (allUsers.Any(u => u.Email == updateDto.Email && u.Id != userId))
            {
                throw new Exception("Bu e-posta adresi kullanımda.");
            }

            user.Email = updateDto.Email;

            // Veritabanına kaydet
            await _userRepository.UpdateAsync(user);
        }

        // --- 4. PAROLA DEĞİŞTİRME ---
        public async Task ChangePasswordAsync(int userId, ChangePasswordDto passwordDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            // Mevcut şifre doğru mu?
            if (!VerifyPasswordHash(passwordDto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            {
                throw new Exception("Mevcut şifreniz yanlış.");
            }

            // Yeni şifreyi oluştur
            CreatePasswordHash(passwordDto.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            // Veritabanına kaydet
            await _userRepository.UpdateAsync(user);
        }


    






        // --- YARDIMCI METOTLAR ---
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
    {
        // 1. ID'yi ekle
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        
        // 2. Email'i ekle (SENİN KODUNDA BU EKSİKTİ)
        new Claim(ClaimTypes.Email, user.Email ?? ""), 

        // 3. İsmi ekle (Burada sorun yok, doğru yapmışsın)
        new Claim(ClaimTypes.Name, user.Name ?? "")
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
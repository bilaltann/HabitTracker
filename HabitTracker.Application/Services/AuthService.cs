using HabitTracker.Application.DTOs.UserDTOs;
using HabitTracker.Application.Interfaces;
using HabitTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
namespace HabitTracker.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IConfiguration _configuration; // AppSettings'den gizli anahtarı okumak için
        public AuthService(IRepository<User> userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }
        public async Task<User> RegisterAsync(UserRegisterDto registerDto)
        {
            // 1.Bu email ile kullanıcı var mı kontrol et
            // (Generic Repository'ye GetWhere eklemediysek, şimdilik GetAll ile çekip bakıyoruz)
            var users = await _userRepository.GetAllAsync();
            if (users.Any(u => u.Email == registerDto.Email))
            {
                throw new Exception("Bu e-posta adresi zaten kullanımda.");
            }

            // 2. Şifreyi Hash'le
            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // 3. User nesnesini oluştur
            var user = new User
            {
                Email = registerDto.Email,
                Name = registerDto.Name, // Entity'de Name alanı yoksa eklemelisin veya boş geçmelisin.
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedDate = DateTime.Now,
                // Başlangıç değerleri
                CurrentPoints = 0,
                Level = 1
            };

            // 4. Kaydet
            await _userRepository.CreateAsync(user);
            return user;
        }
        public async Task<string> LoginAsync(UserLoginDto loginDto)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == loginDto.Email);

            if (user == null)
                throw new Exception("Kullanıcı bulunamadı.");

            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
                throw new Exception("Şifre hatalı.");

            // Token oluştur
            return CreateToken(user);
        }
        // YARDIMCI METOTLAR

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
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // User ID'yi token içine gömüyoruz
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1), // Token 1 gün geçerli
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }


    }
}

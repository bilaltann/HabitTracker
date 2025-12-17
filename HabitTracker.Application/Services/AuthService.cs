using HabitTracker.Application.DTOs.UserDTOs;
using HabitTracker.Application.Interfaces;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth; // Bunu en üste ekle

namespace HabitTracker.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService; // Mail servisi
        private readonly IMemoryCache _memoryCache;  // hafıza için

        public AuthService(IRepository<User> userRepository, IConfiguration configuration, IMailService mailService, IMemoryCache memoryCache)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _mailService = mailService;
            _memoryCache = memoryCache;
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



        public async Task SendVerificationCodeAsync(string email)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == email);

            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            // 6 Haneli Kod Üret
            string code = new Random().Next(100000, 999999).ToString();

            // Kodu RAM'e Kaydet (15 Dakika Ömürlü)
            // Anahtar: Email, Değer: Kod
            _memoryCache.Set(email, code, TimeSpan.FromMinutes(15));

            // Mail Gönder
            string subject = "Doğrulama Kodunuz 🔐";
            string body = $"<h3>HabitQuest Doğrulama Kodu</h3><p>Şifrenizi sıfırlamak için kodunuz: <b style='font-size:20px'>{code}</b></p><p>Bu kod 15 dakika geçerlidir.</p>";

            await _mailService.SendEmailAsync(email, subject, body);
        }


        // KODU DOĞRULAYAN METOT (verify_code.html için)
        public async Task<bool> VerifyCodeOnlyAsync(string email, string code)
        {
            // RAM'den kodu kontrol et
            if (!_memoryCache.TryGetValue(email, out string cachedCode))
            {
                return false; // Kod süresi dolmuş veya hiç yok
            }

            if (cachedCode != code)
            {
                return false; // Kod yanlış
            }

            return true; // Kod doğru
        }

        // ŞİFREYİ DEĞİŞTİREN METOT (new_password.html için)
        public async Task ResetPasswordWithCodeAsync(ResetPasswordDto resetDto)
        {
            // Güvenlik Önlemi: Kodu tekrar kontrol et (Araya giren olmasın diye)
            if (!_memoryCache.TryGetValue(resetDto.Email, out string cachedCode))
            {
                throw new Exception("Kodun süresi dolmuş. Lütfen tekrar kod isteyin.");
            }

            if (cachedCode != resetDto.Code)
            {
                throw new Exception("Kod hatalı.");
            }

            // Kullanıcıyı bul
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == resetDto.Email);


            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            // Yeni şifreyi Hashle
            CreatePasswordHash(resetDto.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            // Veritabanını güncelle
            await _userRepository.UpdateAsync(user);

            // İşlem bitince kodu sil (Tek kullanımlık olsun)
            _memoryCache.Remove(resetDto.Email);
        }


        public async Task<string> LoginWithGoogleAsync(string credential)
        {
            
                // 1. Google Token'ını Doğrula
                GoogleJsonWebSignature.Payload payload;
                
                    var settings = new GoogleJsonWebSignature.ValidationSettings()
                    {
                        // appsettings.json'dan ClientId'yi alıyoruz
                        Audience = new List<string>() { _configuration["GoogleAuthSettings:ClientId"] }
                    };

                    payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);
                
            

                // 2. Kullanıcı Veritabanında Var mı Kontrol Et (Repository ile)
                var users = await _userRepository.GetAllAsync();
                var user = users.FirstOrDefault(u => u.Email == payload.Email);

                if (user == null)
                {
                    // KULLANICI YOKSA -> OTOMATİK KAYIT ET
                    // Google kullanıcıları için şifre olmadığı için rastgele Hash/Salt atıyoruz
                    // ki veritabanı hatası almayalım.
                    CreatePasswordHash(Guid.NewGuid().ToString(), out byte[] dummyHash, out byte[] dummySalt);

                    user = new User
                    {
                        Email = payload.Email,
                        Name = payload.Name, // Google'dan gelen isim
                        PasswordHash = dummyHash, // Rastgele şifre (Google ile girdiği için kullanmayacak)
                        PasswordSalt = dummySalt,
                        CurrentPoints = 0,
                        Level = 1
                    };

                    // Kullanıcıyı oluştur
                    await _userRepository.CreateAsync(user);
                }

                // 3. Kullanıcı artık var, Kendi JWT Token'ını Üret
                // Mevcut 'CreateToken' metodunu kullanıyoruz
                var token = CreateToken(user);

                return token;
            
        }
        public async Task DeleteAccountAsync(int userId)
        {
            // 1. Kullanıcı var mı?
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            // 2. İlişkili verilerin silinmesi (Cascade Delete)
            // Eğer veritabanında Cascade Delete açıksa (EF Core default davranışı),
            // kullanıcıyı sildiğinde ona ait Habit, HabitLog, Friendship vb. her şey otomatik silinir.
            // Eğer Cascade kapalıysa (Restrict ise) önce onları manuel silmen gerekir.
            // Biz varsayılan (Cascade) olduğunu varsayarak direkt siliyoruz:

            await _userRepository.DeleteAsync(userId);
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
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        
                new Claim(ClaimTypes.Email, user.Email ?? ""), 

                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.Role, user.Role)
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
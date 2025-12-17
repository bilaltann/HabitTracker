using HabitTracker.Domain.Entities;
using HabitTracker.Infrastructure.Contexts;

namespace HabitTracker.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        // DbContext'i Scope içinden alacağız, o yüzden constructor'da değil invoke'da çağıracağız.

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IServiceProvider serviceProvider)
        {
            try
            {
                // İsteği devam ettir
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // HATA OLURSA YAKALA VE VERİTABANINA YAZ
                await HandleExceptionAsync(httpContext, ex, serviceProvider);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex, IServiceProvider serviceProvider)
        {
            // Middleware içinden DbContext çağırmak için Scope oluşturuyoruz
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<HabitTrackerDbContext>();

                var log = new SystemLog
                {
                    Level = "Error",
                    Message = ex.Message,
                    StackTrace = ex.StackTrace, // Hatanın yeri
                    CreatedDate = DateTime.Now
                };

                dbContext.SystemLogs.Add(log);
                await dbContext.SaveChangesAsync();
            }

            // Kullanıcıya standart bir hata dön (Frontend 500 hatası alsın)
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { message = "Sunucu hatası oluştu. Log kaydedildi." });
        }
    }
}

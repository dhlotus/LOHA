using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using LOHA.Models;
using System.Threading.Tasks;

namespace LOHA.Middleware
{
    public class CheckUserStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckUserStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // Kiểm tra nếu user đã đăng nhập
            var userSession = context.Session.GetString("user");
            if (!string.IsNullOrEmpty(userSession))
            {
                var user = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

                // Nếu user bị khóa -> xóa session và redirect
                if (user != null && !user.TrangThai)
                {
                    context.Session.Clear();
                    context.Response.Redirect("/User/DangNhap?error=blocked");
                    return;
                }
            }

            await _next(context);
        }
    }
}
using LOHA.Models;
using System.Linq;

namespace LOHA.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Đảm bảo database đã được tạo
            context.Database.EnsureCreated();

            // Tạo tài khoản Lotus Admin mặc định nếu chưa có
            if (!context.Lotuss.Any())
            {
                var admin = new Lotus
                {
                    TenDangNhap = "buiduchalotus",
                    MatKhau = "admin.loha.lotus",
                    HoTen = "LOTUS Administrator",
                    Email = "buiducha@loha.lotus",
                    NgayTao = DateTime.Now,
                    TrangThai = true
                };
                context.Lotuss.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
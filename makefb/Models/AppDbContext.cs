using Microsoft.EntityFrameworkCore;

namespace makefb.Models
{
    public class AppDbContext : DbContext
    {
        // contructor nhận cấu hình database
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
        // bảng trong database
        public DbSet<User> Users { get; set; }
        public DbSet<Baiviet> Baiviets { get; set; }
    }
}

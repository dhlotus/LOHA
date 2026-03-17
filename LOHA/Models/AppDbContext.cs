using Microsoft.EntityFrameworkCore;

namespace LOHA.Models
{
    public class AppDbContext : DbContext
    {
        // contructor nhận cấu hình database
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
        // bảng trong database
        public DbSet<User> Users { get; set; }
        public DbSet<Baiviet> Baiviets { get; set; }
        public DbSet<Binhluan> Binhluans { get; set; } // tạo bảng Binhluans trong db
        
        public DbSet<Thich> Thichs { get; set; } // tạo bảng Thiches trong db
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ❗ bắt buộc

            // 🟢 Binhluan
            modelBuilder.Entity<Binhluan>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Binhluan>()
                .HasOne(b => b.Baiviet)
                .WithMany(bv => bv.Binhluans) // ✅ QUAN TRỌNG
                .HasForeignKey(b => b.BaivietId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🟡 Thich
            modelBuilder.Entity<Thich>()
                .HasOne(t => t.Baiviet)
                .WithMany(b => b.Thichs)
                .HasForeignKey(t => t.BaivietId);

            modelBuilder.Entity<Thich>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.NoAction); // 🔥 THÊM DÒNG NÀY
        }
    }
}

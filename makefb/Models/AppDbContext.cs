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
        public DbSet<Binhluan> Binhluans { get; set; } // tạo bảng Binhluans trong db
        protected override void OnModelCreating(ModelBuilder modelBuilder) //cấu hình quan hệ db thủ công
        {
            modelBuilder.Entity<Binhluan>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Binhluan>()
                .HasOne(b => b.Baiviet)
                .WithMany()
                .HasForeignKey(b => b.BaivietId)
                .OnDelete(DeleteBehavior.Cascade); // xoá bài viết sẽ xoá bình luận liên quan
        }
    }
}

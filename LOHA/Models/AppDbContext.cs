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
        public DbSet<KetBan> KetBans { get; set; } // tạo bảng KetBans trong db
        public DbSet<TinNhan> TinNhans { get; set; } // tạo bảng TinNhans trong db
        public DbSet<DatLaiMatKhau> DatLaiMatKhau { get; set; }
        public DbSet<XacThucEmail> XacThucEmails { get; set; }
        public DbSet<Lotus> Lotuss { get; set; }
        public DbSet<BaoCaoBaiViet> BaoCaoBaiViets { get; set; }
        public DbSet<BaoCaoNguoiDung> BaoCaoNguoiDungs { get; set; }
        public DbSet<NhatKyHoatDongAdmin> NhatKyHoatDongAdmins { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

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
                .OnDelete(DeleteBehavior.NoAction); 
                                                    // Báo cáo bài viết
            modelBuilder.Entity<BaoCaoBaiViet>()
                .HasOne(b => b.NguoiBaoCao)
                .WithMany()
                .HasForeignKey(b => b.NguoiBaoCaoId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BaoCaoBaiViet>()
                .HasOne(b => b.BaiViet)
                .WithMany()
                .HasForeignKey(b => b.BaiVietId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa bài viết → Xóa luôn báo cáo

            // Báo cáo người dùng
            modelBuilder.Entity<BaoCaoNguoiDung>()
                .HasOne(b => b.NguoiBaoCao)
                .WithMany()
                .HasForeignKey(b => b.NguoiBaoCaoId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BaoCaoNguoiDung>()
                .HasOne(b => b.NguoiBiBaoCao)
                .WithMany()
                .HasForeignKey(b => b.NguoiBiBaoCaoId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

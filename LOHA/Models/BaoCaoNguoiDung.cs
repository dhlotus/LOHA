using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOHA.Models
{
    /// <summary>
    /// Bảng lưu báo cáo người dùng
    /// </summary>
    [Table("BaoCaoNguoiDung")]
    public class BaoCaoNguoiDung
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NguoiBaoCaoId { get; set; } // FK → Users (người gửi báo cáo)

        [Required]
        public int NguoiBiBaoCaoId { get; set; } // FK → Users (người bị báo cáo)

        [Required]
        [MaxLength(200)]
        public string LyDo { get; set; } = ""; // Lý do báo cáo

        [Required]
        public DateTime ThoiGian { get; set; } = DateTime.Now; // Thời gian báo cáo

        [Required]
        public int TrangThai { get; set; } = 0; // 0: Chờ xử lý, 1: Đã xử lý, 2: Từ chối

        // Navigation properties
        [ForeignKey("NguoiBaoCaoId")]
        public virtual User? NguoiBaoCao { get; set; }

        [ForeignKey("NguoiBiBaoCaoId")]
        public virtual User? NguoiBiBaoCao { get; set; }
    }
}
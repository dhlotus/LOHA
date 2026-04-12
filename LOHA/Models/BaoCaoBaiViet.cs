using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOHA.Models
{
    /// <summary>
    /// Bảng lưu báo cáo bài viết từ người dùng
    /// </summary>
    [Table("BaoCaoBaiViet")]
    public class BaoCaoBaiViet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NguoiBaoCaoId { get; set; } // FK → Users (người gửi báo cáo)

        [Required]
        public int BaiVietId { get; set; } // FK → Baiviets (bài viết bị báo cáo)

        [Required]
        [MaxLength(200)]
        public string LyDo { get; set; } = ""; // Lý do báo cáo

        [Required]
        public DateTime ThoiGian { get; set; } = DateTime.Now; // Thời gian báo cáo

        [Required]
        public int TrangThai { get; set; } = 0; // 0: Chờ xử lý, 1: Đã xử lý (Đồng ý), 2: Từ chối

        // Navigation properties
        [ForeignKey("NguoiBaoCaoId")]
        public virtual User? NguoiBaoCao { get; set; }

        [ForeignKey("BaiVietId")]
        public virtual Baiviet? BaiViet { get; set; }
    }
}
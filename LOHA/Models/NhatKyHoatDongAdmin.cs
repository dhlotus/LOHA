using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOHA.Models
{
    /// <summary>
    /// Bảng lưu nhật ký hoạt động của Admin
    /// </summary>
    [Table("NhatKyHoatDongAdmin")]
    public class NhatKyHoatDongAdmin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string HanhDong { get; set; } = ""; // Loại hành động: KHOA, MO_KHOA, XOA_BAI, DONG_Y_BAO_CAO...

        [Required]
        [MaxLength(500)]
        public string MoTa { get; set; } = ""; // Mô tả chi tiết: "Admin đã khóa tài khoản Nguyễn Văn A (ID: 5)"

        [Required]
        [MaxLength(100)]
        public string DoiTuong { get; set; } = ""; // Đối tượng bị tác động: "User #5", "Bài viết #123"

        [MaxLength(100)]
        public string? AdminThucHien { get; set; } // Tên admin thực hiện (lotus)

        [Required]
        public DateTime ThoiGian { get; set; } = DateTime.Now; // Thời gian thực hiện

        [MaxLength(50)]
        public string? LoaiDoiTuong { get; set; } // "User", "BaiViet", "BaoCao"
    }
}
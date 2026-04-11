using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOHA.Models
{
    /// <summary>
    /// Bảng lưu thông tin yêu cầu đặt lại mật khẩu
    /// </summary>
    [Table("DatLaiMatKhau")]
    public class DatLaiMatKhau
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(6, MinimumLength = 6)] // Mã OTP 6 số
        public string MaOTP { get; set; } = ""; // ← Sửa tên thành MaOTP

        [Required]
        public DateTime ThoiGianTao { get; set; }

        [Required]
        public DateTime ThoiGianHetHan { get; set; } // Hết hạn sau 5 phút

        public bool DaSuDung { get; set; } = false;
    }
}
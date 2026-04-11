using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOHA.Models
{
    /// <summary>
    /// Bảng lưu thông tin xác thực email khi đăng ký
    /// </summary>
    [Table("XacThucEmail")]
    public class XacThucEmail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = ""; // Email cần xác thực

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string MaOTP { get; set; } = ""; // Mã OTP 6 số

        [Required]
        public string Ten { get; set; } = ""; // Lưu tạm tên người dùng

        [Required]
        public string Matkhau { get; set; } = ""; // Lưu tạm mật khẩu

        public DateTime? Ngaysinh { get; set; } // Lưu tạm ngày sinh

        public string? Gioitinh { get; set; } // Lưu tạm giới tính

        [Required]
        public DateTime ThoiGianTao { get; set; } // Thời điểm tạo OTP

        [Required]
        public DateTime ThoiGianHetHan { get; set; } // OTP hết hạn sau 5 phút

        public bool DaSuDung { get; set; } = false; // Đã xác thực chưa?
    }
}
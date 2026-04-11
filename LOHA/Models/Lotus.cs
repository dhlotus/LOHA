using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOHA.Models
{
    [Table("Lotuses")]  // Tên bảng trong database
    public class Lotus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenDangNhap { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string MatKhau { get; set; } = "";  // ← SỬA: bỏ static, thêm = ""

        [Required]
        [MaxLength(100)]
        public string HoTen { get; set; } = "";

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? LanCuoiDangNhap { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}
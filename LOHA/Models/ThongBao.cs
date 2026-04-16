// Models/ThongBao.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOHA.Models
{
    public class ThongBao
    {
        [Key]
        public int Id { get; set; }

        // Người nhận thông báo (chủ bài viết)
        [Required]
        public int UserId { get; set; }

        // Loại thông báo: "like" hoặc "comment"
        [Required]
        [MaxLength(20)]
        public string Loai { get; set; } = string.Empty;

        // ID bài viết liên quan
        [Required]
        public int BaiVietId { get; set; }

        // Số lượng người đã tương tác (được gộp)
        public int SoLuong { get; set; } = 1;

        // Thời gian tạo (lần đầu tiên)
        public DateTime ThoiGianTao { get; set; } = DateTime.Now;

        // Thời gian cập nhật (khi có người mới tương tác)
        public DateTime ThoiGianCapNhat { get; set; } = DateTime.Now;

        // Đã đọc chưa
        public bool DaDoc { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("BaiVietId")]
        public virtual Baiviet? BaiViet { get; set; }
    }
}
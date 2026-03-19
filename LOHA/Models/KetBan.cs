// BÙI ĐỨC HÀ - LOTUS
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOHA.Models
{
    [Table("KetBans")] // Tên bảng trong database
    public class KetBan
    {
        [Key] // Khóa chính
        public int Id { get; set; }

        // ID người gửi lời mời
        [Required]
        public int NguoiGuiId { get; set; }

        // ID người nhận lời mời
        [Required]
        public int NguoiNhanId { get; set; }

        // Trạng thái kết bạn
        // 0: Đang chờ xác nhận
        // 1: Đã chấp nhận (bạn bè)
        // 2: Đã từ chối
        // 3: Hủy kết bạn
        public int TrangThai { get; set; } = 0; // Mặc định là "Đang chờ"

        // Thời gian gửi lời mời
        public DateTime NgayGui { get; set; } = DateTime.Now;

        // Thời gian phản hồi (chấp nhận/từ chối)
        public DateTime? NgayPhanHoi { get; set; }

        // liên kết với bảng Users (người gửi)
        [ForeignKey("NguoiGuiId")]
        public virtual User? NguoiGui { get; set; }
        // liên kết tới bảng User (người nhận)

        [ForeignKey("NguoiNhanId")]
        public virtual User? NguoiNhan { get; set; }
    }
}
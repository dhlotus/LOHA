using System.ComponentModel.DataAnnotations;
namespace makefb.Models
{
    public class Baiviet
    {
        public int Id { get; set; } // khoá chính bài viết
        public string? Noidung { get; set; } // stutus
        public string? Anh {  get; set; } // tải ảnh nếu user upload
        public DateTime Ngaydang { get; set; } // thời gian đăng
        public int UserId { get; set; }
        public User? User { get; set; } // thuộc tính điều hướng
    }
}

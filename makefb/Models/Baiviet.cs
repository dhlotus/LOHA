using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace makefb.Models
{
    public class Baiviet
    {
        public int Id { get; set; } // khoá chính bài viết
        public string? Noidung { get; set; } // stutus
        public string? Anh { get; set; } // tải ảnh nếu user upload
        public DateTime Ngaydang { get; set; } // thời gian đăng
        public int UserId { get; set; }
        public int Luotthich { get; set; } = 0; // số lượt thích
        public User? User { get; set; } // thuộc tính điều hướng
        public List<Binhluan> Binhluans { get; set; } = new List<Binhluan>();
    }
}

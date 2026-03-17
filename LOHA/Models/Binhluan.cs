using System;
using System.ComponentModel.DataAnnotations;
namespace LOHA.Models
{
    public class Binhluan
    {
        public int Id { get; set; } // khóa chính

        public string? Noidung { get; set; } // nội dung bình luận

        public DateTime Ngaydang { get; set; } = DateTime.Now;

        public int UserId { get; set; } // khóa ngoại user

        public int BaivietId { get; set; } // khóa ngoại bài viết

        public User? User { get; set; } // liên kết user

        public Baiviet? Baiviet { get; set; } // liên kết bài viết
    }
}

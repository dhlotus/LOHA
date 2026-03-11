using System.ComponentModel.DataAnnotations;
namespace makefb.Models
{
    public class Binhluan
    {
        public int Id { get; set; } // khoá chính bình luận
        public string? Noidung { get; set; } // nội dung bình luận
        public DateTime Ngaydang { get; set; } // thời gian bình luận
        public int UserId { get; set; }
        public int BaivietId { get; set; }
        public User? User { get; set; } // thuộc tính điều hướng đến user
        public Baiviet? Baiviet { get; set; } // thuộc tính điều hướng đến bài viết
    }
}

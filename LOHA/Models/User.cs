using System;
using System.ComponentModel.DataAnnotations;
namespace LOHA.Models
{
    public class User
    {
        public int ID { get; set; } // khóa chính
        [Required(ErrorMessage ="Tên không được để trống")] //Hiển thị thông báo không hợp lệ
        public string Ten { get; set; }
        [Required(ErrorMessage ="Ngày sinh không hợp lệ")]
        public DateTime Ngaysinh { get; set; }
        [Required(ErrorMessage ="Giới tính không được để trống")]
        public string Gioitinh { get; set; }
        [Required(ErrorMessage ="Email/SĐT không được bỏ trống")]
        public string EmailorSDT { get; set; }
        [Required(ErrorMessage ="Mật khẩu không được bỏ trống")]
        public string Matkhau { get; set; }
        public string? Avatar { get; set; }
        public List<Baiviet>? Baiviets { get; set; }
    }
}

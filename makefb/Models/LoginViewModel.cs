using System.ComponentModel.DataAnnotations;

namespace makefb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email hoặc số điện thoại không được để trống")]
        public string EmailorSDT { get; set; }

        [Required(ErrorMessage ="Mật khẩu không được để trống")]
        public string Matkhau { get; set; }
    }
}
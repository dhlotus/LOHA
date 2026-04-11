// BÙI ĐỨC HÀ --- LOTUS 
using LOHA.Models;
using System.ComponentModel.DataAnnotations;

namespace LOHA.Models
{
    public class User
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [RegularExpression(@"^[\p{L} ]{2,}$", ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
        [MaxLength(100, ErrorMessage = "Tên không được quá 100 ký tự")]
        public string? Ten { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        [CustomValidation(typeof(User), nameof(ValidateNgaySinh))]
        public DateTime? Ngaysinh { get; set; }

        [Required(ErrorMessage = "Giới tính không được để trống")]
        public string? Gioitinh { get; set; }

        [Required(ErrorMessage = "Email/SĐT không được để trống")]
        [CustomValidation(typeof(User), nameof(ValidateEmailOrPhone))]
        public string? EmailorSDT { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Mật khẩu phải bao gồm cả chữ và số")]
        [DataType(DataType.Password)]
        public string? Matkhau { get; set; }

        public string? Avatar { get; set; }
        public DateTime? Ngaytao { get; set; }
        public List<Baiviet>? Baiviets { get; set; }

        

        public string? AnhNen { get; set; }
        
        
        public DateTime? NgayCapNhatTen { get; set; } // Thời gian cập nhật của tên
        public DateTime? NgayCapNhatNgaySinh { get; set; } // Thời gian cập nhật của ngày sinh
        // Validation tùy chỉnh cho ngày sinh
        public static ValidationResult? ValidateNgaySinh(DateTime? ngaySinh, ValidationContext context)
        {
            if (ngaySinh == null)
                return new ValidationResult("Vui lòng nhập ngày sinh");

            var today = DateTime.Today;
            var age = today.Year - ngaySinh.Value.Year;
            if (ngaySinh.Value.Date > today.AddYears(-age)) age--;

            if (age < 13)
                return new ValidationResult("Bạn phải từ 13 tuổi trở lên để đăng ký");

            if (ngaySinh.Value > today)
                return new ValidationResult("Ngày sinh không được lớn hơn ngày hiện tại");

            return ValidationResult.Success;
        }

        // Validation tùy chỉnh cho email hoặc số điện thoại
        public static ValidationResult? ValidateEmailOrPhone(string? value, ValidationContext context)
        {
            if (string.IsNullOrEmpty(value))
                return new ValidationResult("Vui lòng nhập email hoặc số điện thoại");

            // Kiểm tra email
            if (value.Contains("@"))
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(value);
                    if (addr.Address == value)
                        return ValidationResult.Success;
                }
                catch
                {
                    return new ValidationResult("Email không hợp lệ");
                }
            }
            // Kiểm tra số điện thoại (10 số, bắt đầu bằng 0)
            else if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^0[0-9]{9}$"))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Email không hợp lệ hoặc số điện thoại phải bắt đầu bằng 0 và đủ 10 số");
        }
    }
}


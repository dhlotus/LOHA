// BÙI ĐỨC HÀ --- LOTUS 
using LOHA.Models;
using System.ComponentModel.DataAnnotations;

namespace LOHA.Models
{
    public class User
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(50, ErrorMessage = "Tên không được quá 50 ký tự")]
        [CustomValidation(typeof(User), nameof(ValidateTen))]
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
        public bool TrangThai { get; set; } = true; // true: Hoạt động, false: Đã khóa
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

            if (age < 10)
                return new ValidationResult("Bạn phải từ 10 tuổi trở lên để đăng ký");

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
        // Validation tùy chỉnh cho tên
        public static ValidationResult? ValidateTen(string? ten, ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(ten))
                return new ValidationResult("Vui lòng nhập họ tên");

            // 1. Kiểm tra không chứa chữ số
            if (ten.Any(char.IsDigit))
                return new ValidationResult("Tên không được chứa chữ số");

            // 2. Tách thành các từ (bỏ qua khoảng trắng thừa)
            var words = ten.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 3. Kiểm tra số lượng từ (2-5 từ)
            if (words.Length < 2)
                return new ValidationResult("Họ tên phải có ít nhất 2 từ (ví dụ: Nguyễn Văn A)");

            if (words.Length > 5)
                return new ValidationResult("Họ tên không được quá 5 từ");

            // 4. Kiểm tra mỗi từ có ít nhất 1 ký tự chữ cái
            foreach (var word in words)
            {
                if (word.Length < 1)
                    return new ValidationResult("Mỗi từ trong tên phải có ít nhất 1 ký tự");

                // Kiểm tra từ không chứa ký tự đặc biệt (chỉ cho phép chữ cái)
                if (!word.All(c => char.IsLetter(c) || c == '\'' || c == '-'))
                    return new ValidationResult("Tên chỉ được chứa chữ cái, dấu cách, dấu ' hoặc dấu -");
            }

            return ValidationResult.Success;
        }
    }
}


using LOHA.Models; // dùng các class trong thư mục model
using Microsoft.AspNetCore.Identity; // mã hoá mật khẩu
using Microsoft.AspNetCore.Mvc; // dùng các chức năng của asp
using Microsoft.EntityFrameworkCore; // dùng các chức năng của entity framework
namespace LOHA.Controllers // nhóm chứa các class

{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        // contructor
        public UserController(AppDbContext context)
        {
            _context = context; // biến làm việc với db
        }

        // trang đăng ký
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost] // chạy khi ấn nút đăng ký
        public IActionResult DangKy(User user) // model binding tự ánh xạ đến từng biến
        {
            if(ModelState.IsValid) // kiểm tra form hợp lệ không
            {
                // kiểm tra email sđt đã tồn tại chưa
                var check = _context.Users.FirstOrDefault(x => x.EmailorSDT == user.EmailorSDT);

                if (check != null) {
                    ModelState.AddModelError("EmailorSDT", "Email hoặc số điện thoại đã tồn tại");
                    return View(user);
                }
                // mã hoá mật khẩu
                var hasher = new PasswordHasher<User>();
                user.Matkhau = hasher.HashPassword(user, user.Matkhau);

                _context.Users.Add(user); // thêm user vào database
                _context.SaveChanges(); // lưu thay đổi
                return RedirectToAction("DangKy"); // trả về trang đăng ký
            }
            return View(user); // chưa nhập đủ trả lại trang nhưng vẫn giữ lại dữ liệu
        }
        
        // trang đăng nhập
        public IActionResult DangNhap()
        {
            return View();
        }

        [HttpPost] // chạy khi gửi dữ liệu bằng post
        public IActionResult DangNhap(LoginViewModel model) // tự động gán dữ liệu từ form vào model
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var user = _context.Users
                .FirstOrDefault(x => x.EmailorSDT == model.EmailorSDT); // tìm user trong db
            if(user == null) // kiểm tra mật khẩu
            {
                ModelState.AddModelError("", "Sai email hoặc mật khẩu"); // thêm lỗi vào modelview
                return View(model); // quay lại login giữa lại dữ liệu user đã nhập
            }

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword( // xác minh mật khẩu đã mã hoá
                user, // user trong
                user.Matkhau, // pass đã hash trong db
                model.Matkhau // pass user nhập
                );
            if (result == PasswordVerificationResult.Failed) { // nếu nhập sai mật khẩu
                ModelState.AddModelError("", "Sai email hoặc mật khẩu");
                return View(model);
            }
            HttpContext.Session.SetString("user", user.EmailorSDT); // lưu sesstion khi login thành công
            return RedirectToAction("Trangcanhan"); // nếu đúng chuyển sang trang home
        }

        // đăng xuất
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear(); // thống tin request hiện tại, truy cập sesstion, xoá toàn bộ dữ liệu trong sesstion => xoá thông tin đăng nhập
            return RedirectToAction("DangNhap"); // chuyển về trang đăng nhập
        }
        //trang cá nhân
        public IActionResult Trangcanhan()
        {
            var userSession = HttpContext.Session.GetString("user"); // lấy thông tin đăng nhập từ sesstion

            if (userSession == null) // trả về trang đăng nhập nếu chưa đăng nhập
            {
                return RedirectToAction("DangNhap");
            }

            var user = _context.Users.
                FirstOrDefault(x => x.EmailorSDT == userSession); // lấy thTin user trong db

            //var user = _context.Users.FirstOrDefault(); // tạm để thiết kế UI thôi

            if (user == null) // nếu không tìm thấy user trong db trả về trang đăng nhập
            {
                return RedirectToAction("DangNhap");
            }

            var baiviets = _context.Baiviets
                .Where(b => b.UserId == user.ID)
                .Include(b => b.Binhluans)
                .Include(b => b.Thichs) // ✅ THÊM DÒNG NÀY
                .ToList();

            ViewBag.Baiviets = baiviets;

            return View(user);   // QUAN TRỌNG
        }
    }
}

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

        [HttpPost]
        public IActionResult DangKy(User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email/sdt đã tồn tại chưa
                var existingUser = _context.Users.FirstOrDefault(u => u.EmailorSDT == user.EmailorSDT);
                if (existingUser != null)
                {
                    ModelState.AddModelError("EmailorSDT", "Email hoặc số điện thoại đã được sử dụng");
                    return View(user);
                }

                // Thêm user mới
                user.Ngaytao = DateTime.Now;
                _context.Users.Add(user);
                _context.SaveChanges();

                // Lưu thông báo thành công vào TempData
                TempData["DangKyThanhCong"] = "true";
                TempData["ThongBao"] = "Đăng ký tài khoản thành công!";

                // Chuyển hướng về trang đăng nhập
                return RedirectToAction("DangNhap");
            }
            return View(user);
        }

        // trang đăng nhập
        public IActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DangNhap(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users
                .FirstOrDefault(x => x.EmailorSDT == model.EmailorSDT);

            if (user == null || user.Matkhau != model.Matkhau) // So sánh trực tiếp
            {
                ModelState.AddModelError("", "Sai email hoặc mật khẩu");
                return View(model);
            }

            if (user?.EmailorSDT != null)
            {
                HttpContext.Session.SetString("user", user.EmailorSDT);
            }
            return RedirectToAction("Trangcanhan");
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
            // Lấy email/sdt từ session
            var userSession = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userSession))
                return RedirectToAction("DangNhap");

            // Tìm user trong DB
            var user = _context.Users.FirstOrDefault(u => u.EmailorSDT == userSession);
            if (user == null)
                return RedirectToAction("DangNhap");

            // Lấy danh sách bài viết của user này
            var baiviets = _context.Baiviets
                .Include(b => b.Thichs)   // cần để đếm like
                .Where(b => b.UserId == user.ID)
                .OrderByDescending(b => b.Ngaydang)
                .ToList();

            // Lấy danh sách ID bài viết mà user hiện tại đã like
            var thichs = _context.Thichs
                .Where(t => t.UserId == user.ID)
                .Select(t => t.BaivietId)
                .ToList();

            ViewBag.Baiviets = baiviets;
            ViewBag.Thichs = thichs;   

            return View(user);
        }

    }
}

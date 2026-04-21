using Microsoft.AspNetCore.Mvc;
using LOHA.Models;
using Microsoft.EntityFrameworkCore;

namespace LOHA.Controllers.Admin
{
    /// <summary>
    /// Controller xử lý đăng nhập và quản lý cho Lotus Admin
    /// </summary>
    public class BangDieuKhienController : Controller
    {
        private readonly AppDbContext _context;

        public BangDieuKhienController(AppDbContext context)
        {
            _context = context;
        }

        // ===== TRANG ĐĂNG NHẬP LOTUS =====
        [HttpGet]
        public IActionResult DangNhap()
        {
            // Nếu đã đăng nhập admin rồi thì vào thẳng Dashboard
            var lotusSession = HttpContext.Session.GetString("lotus");
            if (!string.IsNullOrEmpty(lotusSession))
            {
                return RedirectToAction("Index", "BangDieuKhien");
            }
            return View();
        }

        // ===== XỬ LÝ ĐĂNG NHẬP =====
        [HttpPost]
        public async Task<IActionResult> DangNhap(string tenDangNhap, string matKhau)
        {
            // 1. Kiểm tra đầu vào
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            // 2. Tìm admin trong database
            var lotus = await _context.Lotuss
                .FirstOrDefaultAsync(l => l.TenDangNhap == tenDangNhap && l.TrangThai == true);

            if (lotus == null)
            {
                ViewBag.Loi = "Sai tên đăng nhập hoặc mật khẩu";
                return View();
            }

            // 3. Kiểm tra mật khẩu (hỗ trợ cả plain text cũ và hash mới)
            bool isPasswordValid = false;

            // KIỂM TRA XEM MẬT KHẨU ĐÃ ĐƯỢC HASH CHƯA
            // Hash BCrypt luôn bắt đầu bằng $2a$, $2b$, hoặc $2y$
            if (lotus.MatKhau.StartsWith("$2a$") || lotus.MatKhau.StartsWith("$2b$") || lotus.MatKhau.StartsWith("$2y$"))
            {
                // Đã hash → dùng BCrypt.Verify
                try
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(matKhau, lotus.MatKhau);
                }
                catch
                {
                    isPasswordValid = false;
                }
            }
            else
            {
                // Chưa hash (plain text cũ) → so sánh trực tiếp
                isPasswordValid = (lotus.MatKhau == matKhau);

                // Nếu đúng → tự động nâng cấp lên hash
                if (isPasswordValid)
                {
                    lotus.MatKhau = BCrypt.Net.BCrypt.HashPassword(matKhau);
                }
            }

            if (!isPasswordValid)
            {
                ViewBag.Loi = "Sai tên đăng nhập hoặc mật khẩu";
                return View();
            }

            // 4. Cập nhật lần cuối đăng nhập
            lotus.LanCuoiDangNhap = DateTime.Now;
            await _context.SaveChangesAsync();

            // 5. Lưu session Lotus (dùng key "lotus")
            HttpContext.Session.SetString("lotus", lotus.TenDangNhap);
            HttpContext.Session.SetString("lotusHoTen", lotus.HoTen);

            // 6. Chuyển đến Dashboard
            return RedirectToAction("Index", "Dashboard");
        }

        // ===== TRANG DASHBOARD (TỔNG QUAN) =====
        [HttpGet]
        public IActionResult Index()
        {
            // Kiểm tra đăng nhập
            var lotusSession = HttpContext.Session.GetString("lotus");
            if (string.IsNullOrEmpty(lotusSession))
            {
                return RedirectToAction("DangNhap", "BangDieuKhien");
            }

            // Lấy thống kê
            ViewBag.TongNguoiDung = _context.Users.Count();
            ViewBag.TongBaiViet = _context.Baiviets.Count();
            ViewBag.TongBinhLuan = _context.Binhluans.Count();
            ViewBag.TenAdmin = HttpContext.Session.GetString("lotusHoTen") ?? lotusSession;
            return RedirectToAction("Index", "Dashboard"); return View();
        }

        // ===== ĐĂNG XUẤT =====
        public IActionResult DangXuat()
        {
            HttpContext.Session.Remove("lotus");
            HttpContext.Session.Remove("lotusHoTen");
            return RedirectToAction("DangNhap");
        }
    }
}
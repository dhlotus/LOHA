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

            // 3. Kiểm tra tài khoản và mật khẩu
            if (lotus == null || lotus.MatKhau != matKhau)
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
            return RedirectToAction("Index", "BangDieuKhien");
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

            return View();
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
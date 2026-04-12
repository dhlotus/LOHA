using Microsoft.AspNetCore.Mvc;
using LOHA.Models;
using Microsoft.EntityFrameworkCore;

namespace LOHA.Controllers.Admin
{
    /// <summary>
    /// Controller quản lý người dùng cho Lotus Admin
    /// </summary>
    [Route("Admin/QuanLyNguoiDung")]
    public class QuanLyNguoiDungController : Controller
    {
        private readonly AppDbContext _context;

        public QuanLyNguoiDungController(AppDbContext context)
        {
            _context = context;
        }

        // Kiểm tra đăng nhập admin
        private IActionResult KiemTraDangNhap()
        {
            var lotusSession = HttpContext.Session.GetString("lotus");
            if (string.IsNullOrEmpty(lotusSession))
            {
                return RedirectToAction("DangNhap", "BangDieuKhien");
            }
            return null;
        }

        // ===== TRANG DANH SÁCH NGƯỜI DÙNG =====
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index(string searchTerm = "")
        {
            var check = KiemTraDangNhap();
            if (check != null) return check;

            // Lấy danh sách người dùng
            var users = await _context.Users
                .OrderByDescending(u => u.Ngaytao)
                .ToListAsync();

            // Lọc theo từ khóa nếu có
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u =>
                    u.Ten.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.EmailorSDT.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.TenAdmin = HttpContext.Session.GetString("lotus");
            return View(users);
        }
        // ===== KHÓA NGƯỜI DÙNG =====
        [HttpPost]
        [Route("KhoaNguoiDung")]
        public async Task<IActionResult> KhoaNguoiDung(int userId)
        {
            try
            {
                var check = KiemTraDangNhap();
                if (check != null) return Json(new { success = false, message = "Chưa đăng nhập" });

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                user.TrangThai = false;
                await _context.SaveChangesAsync();

                // ===== GHI NHẬT KÝ =====
                await GhiNhatKy(
                    "KHOA",
                    $"Khóa tài khoản {user.Ten} (ID: {user.ID})",
                    $"User #{user.ID}",
                    "User"
                );

                return Json(new { success = true, message = "Đã khóa tài khoản" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ===== MỞ KHÓA NGƯỜI DÙNG =====
        [HttpPost]
        [Route("MoKhoaNguoiDung")]
        public async Task<IActionResult> MoKhoaNguoiDung(int userId)
        {
            try
            {
                var check = KiemTraDangNhap();
                if (check != null) return Json(new { success = false, message = "Chưa đăng nhập" });

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                user.TrangThai = true;
                await _context.SaveChangesAsync();

                // ===== GHI NHẬT KÝ =====
                await GhiNhatKy(
                    "MO_KHOA",
                    $"Mở khóa tài khoản {user.Ten} (ID: {user.ID})",
                    $"User #{user.ID}",
                    "User"
                );

                return Json(new { success = true, message = "Đã mở khóa tài khoản" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        // Helper ghi nhật ký hoạt động
        private async Task GhiNhatKy(string hanhDong, string moTa, string doiTuong, string loaiDoiTuong)
        {
            var lotusSession = HttpContext.Session.GetString("lotus");

            var nhatKy = new NhatKyHoatDongAdmin
            {
                HanhDong = hanhDong,
                MoTa = moTa,
                DoiTuong = doiTuong,
                AdminThucHien = lotusSession ?? "Unknown",
                LoaiDoiTuong = loaiDoiTuong,
                ThoiGian = DateTime.Now
            };

            _context.NhatKyHoatDongAdmins.Add(nhatKy);
            await _context.SaveChangesAsync();
        }
        // ===== XEM CHI TIẾT NGƯỜI DÙNG (GIAO DIỆN ADMIN) =====
        [HttpGet]
        [Route("XemChiTiet/{id}")]
        public async Task<IActionResult> XemChiTiet(int id)
        {
            var check = KiemTraDangNhap();
            if (check != null) return check;

            var user = await _context.Users
                .Include(u => u.Baiviets)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (user == null) return NotFound();

            ViewBag.TenAdmin = HttpContext.Session.GetString("lotus");
            return View(user);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using LOHA.Models;
using Microsoft.EntityFrameworkCore;

namespace LOHA.Controllers.Admin
{
    /// <summary>
    /// Controller cho Dashboard Lotus Admin
    /// </summary>
    [Route("Dashboard")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
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

        // ===== TRANG CHỦ DASHBOARD =====
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            var check = KiemTraDangNhap();
            if (check != null) return check;

            // Lấy dữ liệu thống kê thật từ database
            ViewBag.TongNguoiDung = _context.Users.Count();
            ViewBag.TongBaiViet = _context.Baiviets.Count();
            ViewBag.BaoCaoMoi = _context.BaoCaoBaiViets.Count(b => b.TrangThai == 0)
                              + _context.BaoCaoNguoiDungs.Count(b => b.TrangThai == 0);

            ViewBag.TenAdmin = HttpContext.Session.GetString("lotus");
            return View();
        }

        // ===== API LẤY DANH SÁCH HOẠT ĐỘNG (CÓ PHÂN TRANG) =====
        [HttpGet]
        [Route("LayHoatDong")]
        public async Task<IActionResult> LayHoatDong(int page = 1, int pageSize = 30)
        {
            var check = KiemTraDangNhap();
            if (check != null) return Json(new { success = false, message = "Chưa đăng nhập" });

            // Tính tổng số bản ghi
            int totalItems = await _context.NhatKyHoatDongAdmins.CountAsync();

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Lấy dữ liệu theo trang
            var hoatDongs = await _context.NhatKyHoatDongAdmins
                .OrderByDescending(h => h.ThoiGian)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new
                {
                    h.Id,
                    h.HanhDong,
                    h.MoTa,
                    h.DoiTuong,
                    h.AdminThucHien,
                    ThoiGian = h.ThoiGian.ToString("HH:mm dd/MM/yyyy"),
                    h.LoaiDoiTuong,
                })
                .ToListAsync();

            return Json(new
            {
                success = true,
                data = hoatDongs,
                totalPages = totalPages,
                currentPage = page,
                totalItems = totalItems
            });
        }

        // ===== API LẤY SỐ LIỆU THỐNG KÊ (DÙNG AJAX) =====
        [HttpGet]
        [Route("LayThongKe")]
        public IActionResult LayThongKe()
        {
            var check = KiemTraDangNhap();
            if (check != null) return Json(new { success = false, message = "Chưa đăng nhập" });

            return Json(new
            {
                success = true,
                tongNguoiDung = _context.Users.Count(),
                tongBaiViet = _context.Baiviets.Count(),
                baoCaoMoi = _context.BaoCaoBaiViets.Count(b => b.TrangThai == 0)
                          + _context.BaoCaoNguoiDungs.Count(b => b.TrangThai == 0)
            });
        }
    }
}
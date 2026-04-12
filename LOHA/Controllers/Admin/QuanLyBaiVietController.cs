using Microsoft.AspNetCore.Mvc;
using LOHA.Models;
using Microsoft.EntityFrameworkCore;

namespace LOHA.Controllers.Admin
{
    /// <summary>
    /// Controller quản lý bài viết cho Lotus Admin
    /// </summary>
    [Route("Admin/QuanLyBaiViet")]
    public class QuanLyBaiVietController : Controller
    {
        private readonly AppDbContext _context;

        public QuanLyBaiVietController(AppDbContext context)
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

        // ===== TRANG DANH SÁCH BÀI VIẾT =====
        public async Task<IActionResult> Index()
        {
            var check = KiemTraDangNhap();
            if (check != null) return check;

            // Lấy tất cả bài viết kèm thông tin người đăng
            var baiViets = await _context.Baiviets
                .Include(b => b.User)
                .OrderByDescending(b => b.Ngaydang)
                .ToListAsync();

            ViewBag.TenAdmin = HttpContext.Session.GetString("lotus");
            return View(baiViets);
        }
        // ===== XÓA BÀI VIẾT =====
        [HttpPost]
        public async Task<IActionResult> XoaBaiViet(int id)
        {
            try
            {
                // Kiểm tra đăng nhập admin
                var lotusSession = HttpContext.Session.GetString("lotus");
                if (string.IsNullOrEmpty(lotusSession))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập admin" });
                }

                // Tìm bài viết
                var baiViet = await _context.Baiviets
                    .Include(b => b.User)  // ← lấy tên người đăng
                    .Include(b => b.Binhluans)
                    .Include(b => b.Thichs)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (baiViet == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài viết" });
                }

                // Lưu thông tin để ghi log trước khi xóa
                string tenNguoiDang = baiViet.User?.Ten ?? "Unknown";
                int userId = baiViet.UserId;

                // Xóa ảnh nếu có
                if (!string.IsNullOrEmpty(baiViet.Anh))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/baiviet", baiViet.Anh);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Xóa bài viết
                _context.Baiviets.Remove(baiViet);
                await _context.SaveChangesAsync();

                // ===== GHI NHẬT KÝ =====
                await GhiNhatKy(
                    "XOA_BAI",
                    $"Xóa bài viết #{id} của {tenNguoiDang} (User #{userId})",
                    $"Bài viết #{id}",
                    "BaiViet"
                );

                return Json(new { success = true, message = "Đã xóa bài viết thành công" });
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
        // ===== XEM CHI TIẾT BÀI VIẾT (GIAO DIỆN ADMIN) =====
        [HttpGet]
        [Route("XemChiTiet/{id}")]
        public async Task<IActionResult> XemChiTiet(int id)
        {
            var check = KiemTraDangNhap();
            if (check != null) return check;

            var baiViet = await _context.Baiviets
                .Include(b => b.User)
                .Include(b => b.Thichs)
                .Include(b => b.Binhluans)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (baiViet == null) return NotFound();

            ViewBag.TenAdmin = HttpContext.Session.GetString("lotus");
            return View(baiViet);
        }
    }
}
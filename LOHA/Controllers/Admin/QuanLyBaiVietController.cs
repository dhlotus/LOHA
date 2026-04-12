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
                    .Include(b => b.Binhluans)
                    .Include(b => b.Thichs)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (baiViet == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài viết" });
                }

                // Xóa ảnh nếu có
                if (!string.IsNullOrEmpty(baiViet.Anh))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/baiviet", baiViet.Anh);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Xóa bài viết (EF sẽ tự xóa BinhLuan và Thich liên quan nếu có Cascade)
                _context.Baiviets.Remove(baiViet);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa bài viết thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
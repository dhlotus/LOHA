using Microsoft.AspNetCore.Mvc;
using LOHA.Models;
using Microsoft.EntityFrameworkCore;

namespace LOHA.Controllers.Admin
{
    /// <summary>
    /// Controller quản lý báo cáo bài viết cho Lotus Admin
    /// </summary>
    [Route("Admin/BaoCaoBaiViet")]
    public class BaoCaoBaiVietController : Controller
    {
        private readonly AppDbContext _context;

        public BaoCaoBaiVietController(AppDbContext context)
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

        // ===== TRANG DANH SÁCH BÁO CÁO =====
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var check = KiemTraDangNhap();
            if (check != null) return check;

            // Lấy danh sách báo cáo chưa xử lý (TrangThai = 0)
            var baoCaos = await _context.BaoCaoBaiViets
                .Include(b => b.NguoiBaoCao)
                .Include(b => b.BaiViet)
                    .ThenInclude(bv => bv.User)
                .Where(b => b.TrangThai == 0) // Chỉ lấy báo cáo chưa xử lý
                .OrderByDescending(b => b.ThoiGian)
                .ToListAsync();

            ViewBag.TenAdmin = HttpContext.Session.GetString("lotus");
            return View(baoCaos);
        }
        // ===== XEM TRƯỚC BÀI VIẾT (Trả về HTML) =====
        [HttpGet]
        [Route("XemTruocBaiViet")]
        public async Task<IActionResult> XemTruocBaiViet(int id)
        {
            var check = KiemTraDangNhap();
            if (check != null) return check;

            var baiViet = await _context.Baiviets
                .Include(b => b.User)
                .Include(b => b.Thichs)
                .Include(b => b.Binhluans)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (baiViet == null)
            {
                return Content("<p class='text-danger'>Không tìm thấy bài viết</p>");
            }

            string html = $@"
        <div class='d-flex align-items-start gap-3 mb-3'>
            <img src='/images/default.png' class='rounded-circle' style='width: 50px; height: 50px;' />
            <div>
                <h6 class='mb-1'>{baiViet.User?.Ten}</h6>
                <small class='text-white-50'>{baiViet.Ngaydang.ToString("dd/MM/yyyy HH:mm")}</small>
            </div>
        </div>
        <p class='mb-3'>{baiViet.Noidung}</p>";

            if (!string.IsNullOrEmpty(baiViet.Anh))
            {
                html += $@"
        <div class='text-center mb-3'>
            <img src='/baiviet/{baiViet.Anh}' class='img-fluid rounded' style='max-height: 300px;' />
        </div>";
            }

            html += $@"
        <div class='d-flex gap-4 text-white-50'>
            <span><i class='fa-regular fa-heart me-1'></i>{baiViet.Thichs?.Count ?? 0} thích</span>
            <span><i class='fa-regular fa-comment me-1'></i>{baiViet.Binhluans?.Count ?? 0} bình luận</span>
        </div>";

            return Content(html);
        }

        // ===== ĐỒNG Ý BÁO CÁO (XÓA BÀI VIẾT) =====
        [HttpPost]
        [Route("DongYBaoCao")]
        public async Task<IActionResult> DongYBaoCao(int baoCaoId, int baiVietId)
        {
            try
            {
                var check = KiemTraDangNhap();
                if (check != null) return Json(new { success = false, message = "Chưa đăng nhập" });

                // Tìm báo cáo
                var baoCao = await _context.BaoCaoBaiViets
                    .Include(b => b.NguoiBaoCao)
                    .FirstOrDefaultAsync(b => b.Id == baoCaoId);

                if (baoCao == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy báo cáo" });
                }

                // Tìm bài viết
                var baiViet = await _context.Baiviets
                    .Include(b => b.User)
                    .Include(b => b.Binhluans)
                    .Include(b => b.Thichs)
                    .FirstOrDefaultAsync(b => b.Id == baiVietId);

                string tenNguoiDang = baiViet?.User?.Ten ?? "Unknown";
                int userId = baiViet?.UserId ?? 0;

                if (baiViet != null)
                {
                    // Xóa ảnh nếu có
                    if (!string.IsNullOrEmpty(baiViet.Anh))
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/baiviet", baiViet.Anh); //lấy đường dẫn ảnh của bài viết
                        if (System.IO.File.Exists(imagePath)) // sử lý trong ổ cứng, nếu tồn tại ảnh thì xóa
                        {
                            System.IO.File.Delete(imagePath); // xóa ảnh khỏi ổ cứng
                        }
                    }

                    _context.Baiviets.Remove(baiViet);
                }

                baoCao.TrangThai = 1; // Đánh dấu đã xử lý (TrangThai = 1)
                await _context.SaveChangesAsync();

                // ===== GHI NHẬT KÝ =====
                await GhiNhatKy(
                    "DONG_Y_BAO_CAO",
                    $"Đồng ý báo cáo - Xóa bài viết #{baiVietId} của {tenNguoiDang} (User #{userId})",
                    $"Báo cáo #{baoCaoId}",
                    "BaoCao"
                );

                return Json(new { success = true, message = "Đã xóa bài viết và xử lý báo cáo" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ===== TỪ CHỐI BÁO CÁO =====
        [HttpPost]
        [Route("TuChoiBaoCao")]
        public async Task<IActionResult> TuChoiBaoCao(int baoCaoId)
        {
            try
            {
                var check = KiemTraDangNhap();
                if (check != null) return Json(new { success = false, message = "Chưa đăng nhập" });

                var baoCao = await _context.BaoCaoBaiViets
                    .Include(b => b.NguoiBaoCao)
                    .Include(b => b.BaiViet)
                        .ThenInclude(bv => bv.User)
                    .FirstOrDefaultAsync(b => b.Id == baoCaoId);

                if (baoCao == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy báo cáo" });
                }

                // Lưu thông tin để ghi log
                string tenNguoiBaoCao = baoCao.NguoiBaoCao?.Ten ?? "Unknown";
                string tenNguoiBiBaoCao = baoCao.BaiViet?.User?.Ten ?? "Unknown";
                int baiVietId = baoCao.BaiVietId;

                // Đánh dấu từ chối (TrangThai = 2)
                baoCao.TrangThai = 2;
                await _context.SaveChangesAsync();

                // ===== GHI NHẬT KÝ =====
                await GhiNhatKy(
                    "TU_CHOI_BAO_CAO",
                    $"Từ chối báo cáo bài viết #{baiVietId} của {tenNguoiBiBaoCao} (Người báo cáo: {tenNguoiBaoCao})",
                    $"Báo cáo #{baoCaoId}",
                    "BaoCao"
                );

                return Json(new { success = true, message = "Đã từ chối báo cáo" });
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
    }
}
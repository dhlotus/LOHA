using Microsoft.AspNetCore.Mvc;
using LOHA.Models;
using Microsoft.EntityFrameworkCore;

namespace LOHA.Controllers.Admin
{
    /// <summary>
    /// Controller quản lý báo cáo người dùng cho Lotus Admin
    /// </summary>
    [Route("Admin/BaoCaoNguoiDung")]
    public class BaoCaoNguoiDungController : Controller
    {
        private readonly AppDbContext _context;

        public BaoCaoNguoiDungController(AppDbContext context)
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
            var baoCaos = await _context.BaoCaoNguoiDungs
                .Include(b => b.NguoiBaoCao)
                .Include(b => b.NguoiBiBaoCao)
                .Where(b => b.TrangThai == 0) // Chỉ lấy báo cáo chưa xử lý
                .OrderByDescending(b => b.ThoiGian)
                .ToListAsync();

            ViewBag.TenAdmin = HttpContext.Session.GetString("lotus");
            return View(baoCaos);
        }

        // ===== XEM TRƯỚC NGƯỜI DÙNG (Trả về HTML) =====
        [HttpGet]
        [Route("XemTruocNguoiDung")]
        public async Task<IActionResult> XemTruocNguoiDung(int id)
        {
            var check = KiemTraDangNhap();
            if (check != null) return check;

            var user = await _context.Users
                .Include(u => u.Baiviets)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (user == null)
            {
                return Content("<p class='text-danger'>Không tìm thấy người dùng</p>");
            }

            // Lấy avatar
            string avatar = !string.IsNullOrEmpty(user.Avatar) ? user.Avatar : "/images/default.png";

            // Đếm số bài viết
            int soBaiViet = user.Baiviets?.Count ?? 0;

            // Trạng thái tài khoản
            string trangThaiHienThi = user.TrangThai ? "Hoạt động" : "Đã khóa";
            string badgeClass = user.TrangThai ? "bg-success" : "bg-danger";

            string html = $@"
        <div class='text-center mb-3'>
            <img src='{avatar}' class='rounded-circle' style='width: 80px; height: 80px; object-fit: cover;' />
            <h5 class='mt-2 mb-1'>{user.Ten}</h5>
            <p class='text-white-50 mb-2'>{user.EmailorSDT}</p>
            <span class='badge {badgeClass}'>{trangThaiHienThi}</span>
        </div>
        <hr class='border-secondary' />
        <div class='row text-center'>
            <div class='col-6'>
                <h4 class='mb-0'>{soBaiViet}</h4>
                <small class='text-white-50'>Bài viết</small>
            </div>
            <div class='col-6'>
                <h4 class='mb-0'>{user.Ngaytao?.ToString("dd/MM/yyyy") ?? "N/A"}</h4>
                <small class='text-white-50'>Ngày tham gia</small>
            </div>
        </div>
        <hr class='border-secondary' />
        <div class='text-white-50 small'>
            <p class='mb-1'><span class='material-icons-outlined' style='font-size: 14px;'>cake</span> Ngày sinh: {user.Ngaysinh?.ToString("dd/MM/yyyy") ?? "Chưa cập nhật"}</p>
            <p class='mb-0'><span class='material-icons-outlined' style='font-size: 14px;'>wc</span> Giới tính: {user.Gioitinh ?? "Chưa cập nhật"}</p>
        </div>";

            return Content(html);
        }

        // ===== ĐỒNG Ý BÁO CÁO (KHÓA TÀI KHOẢN) =====
        [HttpPost]
        [Route("DongYBaoCao")]
        public async Task<IActionResult> DongYBaoCao(int baoCaoId, int userId)
        {
            try
            {
                var check = KiemTraDangNhap();
                if (check != null) return Json(new { success = false, message = "Chưa đăng nhập" });

                // Tìm báo cáo
                var baoCao = await _context.BaoCaoNguoiDungs.FindAsync(baoCaoId);
                if (baoCao == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy báo cáo" });
                }

                // Tìm user và khóa tài khoản
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.TrangThai = false; // Khóa tài khoản
                }

                // Cập nhật trạng thái báo cáo thành "Đã xử lý"
                baoCao.TrangThai = 1;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã khóa tài khoản và xử lý báo cáo" });
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

                var baoCao = await _context.BaoCaoNguoiDungs.FindAsync(baoCaoId);
                if (baoCao == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy báo cáo" });
                }

                // Đánh dấu từ chối (TrangThai = 2)
                baoCao.TrangThai = 2;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã từ chối báo cáo" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
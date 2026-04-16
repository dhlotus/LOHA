// Controllers/ThongBaoController.cs
using LOHA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LOHA.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly AppDbContext _context;

        public ThongBaoController(AppDbContext context)
        {
            _context = context;
        }

        // ===== API LẤY DANH SÁCH THÔNG BÁO =====
        [HttpGet]
        public async Task<IActionResult> LayThongBao()
        {
            var userSession = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userSession))
                return Json(new { success = false, data = new List<object>() });

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

            if (currentUser == null)
                return Json(new { success = false, data = new List<object>() });

            // Lấy 10 thông báo gần nhất (chưa đọc lên đầu)
            var thongBaos = await _context.ThongBaos
               .Include(t => t.BaiViet)
               .Where(t => t.UserId == currentUser.ID)
               .OrderByDescending(t => !t.DaDoc)      // Chưa đọc (false) lên trước đã đọc (true)
               .ThenByDescending(t => t.ThoiGianCapNhat) // Mới nhất lên đầu
               .Take(10)
               .Select(t => new
               {
                   t.Id,
                   t.Loai,
                   t.BaiVietId,
                   t.SoLuong,
                   t.DaDoc,
                   ThoiGian = t.ThoiGianCapNhat,
                   NoiDungBaiViet = t.BaiViet != null ? (t.BaiViet.Noidung.Length > 50
                       ? t.BaiViet.Noidung.Substring(0, 50) + "..."
                       : t.BaiViet.Noidung) : ""
               })
               .ToListAsync();

            return Json(new { success = true, data = thongBaos });
        }

        // ===== API ĐẾM THÔNG BÁO CHƯA ĐỌC =====
        [HttpGet]
        public async Task<IActionResult> DemThongBaoChuaDoc()
        {
            var userSession = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userSession))
                return Json(new { count = 0 });

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

            if (currentUser == null)
                return Json(new { count = 0 });

            var count = await _context.ThongBaos
                .CountAsync(t => t.UserId == currentUser.ID && !t.DaDoc);

            return Json(new { count = count });
        }

        // ===== API ĐÁNH DẤU ĐÃ ĐỌC =====
        [HttpPost]
        public async Task<IActionResult> DanhDauDaDoc(int id)
        {
            var thongBao = await _context.ThongBaos.FindAsync(id);
            if (thongBao != null)
            {
                thongBao.DaDoc = true;
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }
    }
}
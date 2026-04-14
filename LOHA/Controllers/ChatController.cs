// BÙI ĐỨC HÀ - LOTUS
using LOHA.Hubs;
using LOHA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LOHA.Controllers
{
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;

        // Constructor - khởi tạo controller với database context
        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        // === TRANG DANH SÁCH TIN NHẮN ===
        // Hiển thị danh sách những người đã từng nhắn tin với mình
        // === TRANG DANH SÁCH TIN NHẮN ===
        // === TRANG DANH SÁCH TIN NHẮN ===
        public async Task<IActionResult> Index()
        {
            // Lấy user hiện tại
            var userSession = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userSession))
                return RedirectToAction("DangNhap", "User");

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

            if (currentUser == null)
                return RedirectToAction("DangNhap", "User");

            // ===== LẤY DANH SÁCH NGƯỜI CÓ TIN NHẮN CHƯA BỊ XÓA =====
            var danhSachUser = new List<User>();
            ViewBag.TinCuoi = new Dictionary<int, TinNhan>();

            // Lấy tất cả bạn bè
            var banBeIds = await _context.KetBans
                .Where(k => (k.NguoiGuiId == currentUser.ID || k.NguoiNhanId == currentUser.ID) && k.TrangThai == 1)
                .Select(k => k.NguoiGuiId == currentUser.ID ? k.NguoiNhanId : k.NguoiGuiId)
                .ToListAsync();

            foreach (var userId in banBeIds)
            {
                // CHỈ LẤY TIN NHẮN CHƯA BỊ XÓA BỞI CURRENT USER
                var tinCuoi = await _context.TinNhans
                    .Where(t => (t.NguoiGuiID == currentUser.ID && t.NguoiNhanID == userId && !t.DaXoaBoiNguoiGui) ||
                               (t.NguoiGuiID == userId && t.NguoiNhanID == currentUser.ID && !t.DaXoaBoiNguoiNhan))
                    .OrderByDescending(t => t.ThoiGian)
                    .FirstOrDefaultAsync();

                // CHỈ THÊM USER VÀO DANH SÁCH NẾU CÓ ÍT NHẤT 1 TIN NHẮN CHƯA XÓA
                if (tinCuoi != null)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        ViewBag.TinCuoi[userId] = tinCuoi;
                        danhSachUser.Add(user);
                    }
                }
            }

            // Nếu không có bạn bè nào có tin nhắn, kiểm tra thêm người đã từng nhắn (không phải bạn bè)
            if (danhSachUser.Count == 0)
            {
                var nguoiDaNhanIds = await _context.TinNhans
                    .Where(t => (t.NguoiGuiID == currentUser.ID && !t.DaXoaBoiNguoiGui) ||
                               (t.NguoiNhanID == currentUser.ID && !t.DaXoaBoiNguoiNhan))
                    .Select(t => t.NguoiGuiID == currentUser.ID ? t.NguoiNhanID : t.NguoiGuiID)
                    .Distinct()
                    .ToListAsync();

                foreach (var userId in nguoiDaNhanIds)
                {
                    // Bỏ qua nếu đã có trong danh sách
                    if (danhSachUser.Any(u => u.ID == userId))
                        continue;

                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        var tinCuoi = await _context.TinNhans
                            .Where(t => (t.NguoiGuiID == currentUser.ID && t.NguoiNhanID == userId && !t.DaXoaBoiNguoiGui) ||
                                       (t.NguoiGuiID == userId && t.NguoiNhanID == currentUser.ID && !t.DaXoaBoiNguoiNhan))
                            .OrderByDescending(t => t.ThoiGian)
                            .FirstOrDefaultAsync();

                        if (tinCuoi != null)
                        {
                            ViewBag.TinCuoi[userId] = tinCuoi;
                            danhSachUser.Add(user);
                        }
                    }
                }
            }

            ViewBag.DanhSachUser = danhSachUser;
            ViewBag.CurrentUserId = currentUser.ID;

            return View();
        }

        // === TRANG CHI TIẾT CHAT VỚI 1 NGƯỜI ===
        // Hiển thị lịch sử tin nhắn giữa mình và 1 người cụ thể
        // === TRANG CHI TIẾT CHAT VỚI 1 NGƯỜI ===

        public async Task<IActionResult> ChatVoi(int userId) // userId là ID của người kia
        {
            // Lấy thông tin user đang đăng nhập
            var userSession = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userSession))
                return RedirectToAction("DangNhap", "User");

            // Tìm currentUser từ session
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

            if (currentUser == null)
                return RedirectToAction("DangNhap", "User");

            // Kiểm tra người nhận có tồn tại không
            var nguoiNhan = await _context.Users.FindAsync(userId);
            if (nguoiNhan == null)
                return NotFound(); // Trả về lỗi 404 nếu không tìm thấy

            // Lấy lịch sử tin nhắn - CHỈ LẤY NHỮNG TIN CHƯA BỊ XÓA BỞI CURRENT USER
            var tinNhans = await _context.TinNhans
                .Where(t => (t.NguoiGuiID == currentUser.ID && t.NguoiNhanID == userId && !t.DaXoaBoiNguoiGui) ||  // Mình gửi và chưa xóa
                            (t.NguoiGuiID == userId && t.NguoiNhanID == currentUser.ID && !t.DaXoaBoiNguoiNhan))   // Mình nhận và chưa xóa
                .OrderBy(t => t.ThoiGian)
                .ToListAsync();
            if (tinNhans == null)
            {
                tinNhans = new List<TinNhan>();
            }
            //  Đánh dấu tin nhắn đã xem
            var tinChuaXem = tinNhans.Where(t => t.NguoiNhanID == currentUser.ID && !t.DaXem).ToList();
            foreach (var tin in tinChuaXem)
            {
                tin.DaXem = true;
            }
            await _context.SaveChangesAsync();

            //  Gửi dữ liệu xuống View
            ViewBag.NguoiNhan = nguoiNhan;
            ViewBag.CurrentUserId = currentUser.ID;

            return View(tinNhans); // Trả về view với danh sách tin nhắn
        }

        // === GỬI TIN NHẮN  ===

        [HttpPost]
        public async Task<IActionResult> GuiTinNhan(int nguoiNhanId, string noiDung)
        {
            try
            {
                var userSession = HttpContext.Session.GetString("user");
                if (string.IsNullOrEmpty(userSession))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var nguoiGui = await _context.Users.FirstOrDefaultAsync(u => u.EmailorSDT == userSession);
                if (nguoiGui == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                // Tạo tin nhắn mới
                var tinNhan = new TinNhan
                {
                    NguoiGuiID = nguoiGui.ID,
                    NguoiNhanID = nguoiNhanId,
                    NoiDung = noiDung,
                    ThoiGian = DateTime.Now,
                    DaXem = false
                };

                _context.TinNhans.Add(tinNhan);
                await _context.SaveChangesAsync();

                // Gọi Hub để gửi tin nhắn realtime
                var chatHub = HttpContext.RequestServices.GetService<IHubContext<ChatHub>>();

                // Tạo roomId
                string roomId = nguoiGui.ID < nguoiNhanId
                    ? $"chat-{nguoiGui.ID}-{nguoiNhanId}"
                    : $"chat-{nguoiNhanId}-{nguoiGui.ID}";

                // Gửi đến group
                await chatHub.Clients.Group(roomId).SendAsync("NhanTinMoi", nguoiGui.ID, nguoiNhanId, noiDung, DateTime.Now);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // === API ĐẾM TIN NHẮN CHƯA ĐỌC ===
        [HttpGet]
        public async Task<IActionResult> DemTinNhanChuaDoc()
        {
            try
            {
                // Lấy user hiện tại
                var userSession = HttpContext.Session.GetString("user");
                if (string.IsNullOrEmpty(userSession))
                {
                    return Json(new { count = 0 });
                }

                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

                if (currentUser == null)
                {
                    return Json(new { count = 0 });
                }

                // Đếm tin nhắn chưa đọc
                var count = await _context.TinNhans
                    .CountAsync(t => t.NguoiNhanID == currentUser.ID && !t.DaXem);

                return Json(new { count = count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }
        // === XÓA TIN NHẮN 1 PHÍA ===
        [HttpPost]  
        public async Task<IActionResult> XoaTinNhan(int nguoiNhanId)  // ← ID của người mình muốn xóa chat
        {
            try
            {
                // ===== BƯỚC 1: KIỂM TRA ĐĂNG NHẬP =====
                var userSession = HttpContext.Session.GetString("user");
                // ↑ Lấy email/SĐT từ Session (đã lưu khi đăng nhập)

                if (string.IsNullOrEmpty(userSession))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                // ===== BƯỚC 2: TÌM USER HIỆN TẠI TRONG DATABASE =====
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);
                // ↑ Tìm user có email/SĐT khớp với session

                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                // ===== BƯỚC 3: LẤY TẤT CẢ TIN NHẮN GIỮA 2 NGƯỜI =====
                var tinNhans = await _context.TinNhans
                    .Where(t => (t.NguoiGuiID == currentUser.ID && t.NguoiNhanID == nguoiNhanId) ||  // Mình gửi cho họ
                               (t.NguoiGuiID == nguoiNhanId && t.NguoiNhanID == currentUser.ID))    // Họ gửi cho mình
                    .ToListAsync();
                // ↑ Lấy TẤT CẢ tin nhắn 2 chiều, không phân biệt ai gửi

                // ===== BƯỚC 4: ĐÁNH DẤU XÓA TÙY VAI TRÒ =====
                foreach (var tin in tinNhans)  // ← Duyệt từng tin nhắn
                {
                    if (tin.NguoiGuiID == currentUser.ID)  // ← Nếu mình là NGƯỜI GỬI
                    {
                        tin.DaXoaBoiNguoiGui = true;  // ← Đánh dấu "người gửi đã xóa"
                    }
                    else  // ← Ngược lại, mình là NGƯỜI NHẬN
                    {
                        tin.DaXoaBoiNguoiNhan = true;  // ← Đánh dấu "người nhận đã xóa"
                    }
                }

                // ===== BƯỚC 5: LƯU THAY ĐỔI VÀO DATABASE =====
                await _context.SaveChangesAsync();

                // ===== BƯỚC 6: TRẢ VỀ KẾT QUẢ THÀNH CÔNG =====
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Nếu có lỗi → trả về thông báo lỗi
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
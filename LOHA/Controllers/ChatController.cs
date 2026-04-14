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

            // ----- CÁCH 1: Lấy danh sách bạn bè (ưu tiên) -----
            // Lấy tất cả bạn bè của user hiện tại
            var banBeIds = await _context.KetBans
                .Where(k => (k.NguoiGuiId == currentUser.ID || k.NguoiNhanId == currentUser.ID) && k.TrangThai == 1)
                .Select(k => k.NguoiGuiId == currentUser.ID ? k.NguoiNhanId : k.NguoiGuiId)
                .ToListAsync();

            var danhSachUser = new List<User>();
            ViewBag.TinCuoi = new Dictionary<int, TinNhan>();

            foreach (var userId in banBeIds)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    // Lấy tin nhắn cuối cùng (nếu có)
                    var tinCuoi = await _context.TinNhans
                        .Where(t => (t.NguoiGuiID == currentUser.ID && t.NguoiNhanID == userId) ||
                                   (t.NguoiGuiID == userId && t.NguoiNhanID == currentUser.ID))
                        .OrderByDescending(t => t.ThoiGian)
                        .FirstOrDefaultAsync();

                    if (tinCuoi != null)
                    {
                        ViewBag.TinCuoi[userId] = tinCuoi;
                    }

                    danhSachUser.Add(user);
                }
            }

            // ----- CÁCH 2: Nếu không có bạn bè thì mới lấy danh sách đã nhắn -----
            if (danhSachUser.Count == 0)
            {
                var danhSachNguoiNhan = await _context.TinNhans
                    .Where(t => t.NguoiGuiID == currentUser.ID || t.NguoiNhanID == currentUser.ID)
                    .Select(t => t.NguoiGuiID == currentUser.ID ? t.NguoiNhanID : t.NguoiGuiID)
                    .Distinct()
                    .ToListAsync();

                foreach (var userId in danhSachNguoiNhan)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        var tinCuoi = await _context.TinNhans
                            .Where(t => (t.NguoiGuiID == currentUser.ID && t.NguoiNhanID == userId) ||
                                       (t.NguoiGuiID == userId && t.NguoiNhanID == currentUser.ID))
                            .OrderByDescending(t => t.ThoiGian)
                            .FirstOrDefaultAsync();

                        if (tinCuoi != null)
                        {
                            ViewBag.TinCuoi[userId] = tinCuoi;
                        }

                        danhSachUser.Add(user);
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
            
            //Lấy lịch sử tin nhắn giữa 2 người (cả 2 chiều)
            var tinNhans = await _context.TinNhans
                .Where(t => (t.NguoiGuiID == currentUser.ID && t.NguoiNhanID == userId) ||
                           (t.NguoiGuiID == userId && t.NguoiNhanID == currentUser.ID))
                .OrderBy(t => t.ThoiGian) // Cũ lên trước, mới xuống sau
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
    }
}
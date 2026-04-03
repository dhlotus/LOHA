// BÙI ĐỨC HÀ - LOTUS
using LOHA.Models; // dùng các class trong thư mục model
using Microsoft.AspNetCore.Identity; // mã hoá mật khẩu
using Microsoft.AspNetCore.Mvc; // dùng các chức năng của asp
using Microsoft.EntityFrameworkCore; // dùng các chức năng của entity framework
namespace LOHA.Controllers // nhóm chứa các class

{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        // contructor
        public UserController(AppDbContext context)
        {
            _context = context; // biến làm việc với db
        }

        // trang đăng ký
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DangKy(User user, string XacNhanMatKhau)
        {
            // Kiểm tra xác nhận mật khẩu
            if (user.Matkhau != XacNhanMatKhau)
            {
                ModelState.AddModelError("Matkhau", "Mật khẩu xác nhận không khớp");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra email/sdt đã tồn tại chưa
                var existingUser = _context.Users.FirstOrDefault(u => u.EmailorSDT == user.EmailorSDT);
                if (existingUser != null)
                {
                    ModelState.AddModelError("EmailorSDT", "Email hoặc số điện thoại đã được sử dụng");
                    return View(user);
                }

                // Thêm user mới
                user.Ngaytao = DateTime.Now;
                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["DangKyThanhCong"] = "true";
                TempData["ThongBao"] = "Đăng ký tài khoản thành công!";

                return RedirectToAction("DangNhap");
            }
            return View(user);
        }

        // trang đăng nhập
        public IActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DangNhap(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users
                .FirstOrDefault(x => x.EmailorSDT == model.EmailorSDT);

            if (user == null || user.Matkhau != model.Matkhau) // So sánh trực tiếp
            {
                ModelState.AddModelError("", "Sai email hoặc mật khẩu");
                return View(model);
            }

            if (user?.EmailorSDT != null)
            {
                HttpContext.Session.SetString("user", user.EmailorSDT);
            }
            return RedirectToAction("Trangcanhan", "User");
        }

        // đăng xuất
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear(); // thống tin request hiện tại, truy cập sesstion, xoá toàn bộ dữ liệu trong sesstion => xoá thông tin đăng nhập
            return RedirectToAction("DangNhap"); // chuyển về trang đăng nhập
        }
        //trang cá nhân
        public async Task<IActionResult> Trangcanhan(int id)
        {
            // Lấy email/sdt từ session
            var userSession = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userSession))
                return RedirectToAction("DangNhap");

            // Tìm user trong DB
            var currentUser = _context.Users.FirstOrDefault(u => u.EmailorSDT == userSession);
            if (currentUser == null)
                return RedirectToAction("DangNhap");

            // nếu không có id thì xem trang cá nhân chính mình
            if (id == 0)
            {
                id = currentUser.ID;
            }

            // tìm user dc xem (có thể là mình hoặc ngkh)
            var user = _context.Users.FirstOrDefault(u => u.ID == id);
            if (user == null)
            {
                return NotFound();
            }

            // Lấy danh sách bài viết của user này
            var baiviets = _context.Baiviets
                .Include(b => b.Thichs)
                .Include(b => b.Binhluans)
                    .ThenInclude(bl => bl.User)
                .Where(b => b.UserId == user.ID)
                .OrderByDescending(b => b.Ngaydang)
                .ToList();

            // Lấy danh sách ID bài viết mà user hiện tại đã like
            var thichs = _context.Thichs
                .Where(t => t.UserId == currentUser.ID)
                .Select(t => t.BaivietId)
                .ToList();

            //tính toán thông tin kết bạn
            bool daLaBanBe = false;
            bool daGuiLoiMoi = false;
            int soBanBe = 0;

            // Nếu đang xem trang của người khác (không phải mình)
            if (currentUser.ID != user.ID)
            {
                // Kiểm tra đã là bạn bè chưa
                daLaBanBe = _context.KetBans.Any(k =>
                    ((k.NguoiGuiId == currentUser.ID && k.NguoiNhanId == user.ID) ||
                     (k.NguoiGuiId == user.ID && k.NguoiNhanId == currentUser.ID)) &&
                    k.TrangThai == 1);

                // Kiểm tra đã gửi lời mời chưa (trạng thái 0: đang chờ)
                if (!daLaBanBe)
                {
                    daGuiLoiMoi = _context.KetBans.Any(k =>
                        k.NguoiGuiId == currentUser.ID &&
                        k.NguoiNhanId == user.ID &&
                        k.TrangThai == 0);
                }
            }
            // Đếm số bạn bè của user này
            soBanBe = _context.KetBans.Count(k =>
                (k.NguoiGuiId == user.ID || k.NguoiNhanId == user.ID) &&
                k.TrangThai == 1);

            // Truyền dữ liệu sang View qua ViewBag
            ViewBag.Baiviets = baiviets;
            ViewBag.Thichs = thichs;
            ViewBag.CurrentUserId = currentUser.ID;      // cho partial view
            ViewBag.DaLaBanBe = daLaBanBe;
            ViewBag.DaGuiLoiMoi = daGuiLoiMoi;
            ViewBag.SoBanBe = soBanBe;

            // Lấy danh sách bạn bè
            var danhSachBanBe = new List<User>();
            var cacKetBan = await _context.KetBans
                .Where(k => (k.NguoiGuiId == user.ID || k.NguoiNhanId == user.ID) && k.TrangThai == 1)
                .ToListAsync();

            ViewBag.NgayKetBan = new Dictionary<int, DateTime>();
            foreach (var ketBan in cacKetBan)
            {
                if (ketBan.NguoiGuiId == user.ID)
                {
                    var ban = await _context.Users.FindAsync(ketBan.NguoiNhanId);
                    if (ban != null)
                    {
                        ViewBag.NgayKetBan[ban.ID] = ketBan.NgayPhanHoi ?? ketBan.NgayGui;
                        danhSachBanBe.Add(ban);
                    }
                }
                else
                {
                    var ban = await _context.Users.FindAsync(ketBan.NguoiGuiId);
                    if (ban != null)
                    {
                        ViewBag.NgayKetBan[ban.ID] = ketBan.NgayPhanHoi ?? ketBan.NgayGui;
                        danhSachBanBe.Add(ban);
                    }
                }
            }
            ViewBag.DanhSachBanBe = danhSachBanBe;

            return View(user);
        }

        // ===== GỬI LỜI MỜI KẾT BẠN =====
        [HttpPost]
        public async Task<IActionResult> GuiLoiMoiKetBan(int nguoiNhanId) // cho phép chạy bất đồng bộ
        {
            try // bắt mọi lỗi sảy ra
            {
                // Lấy thông tin user đang đăng nhập
                var userSession = HttpContext.Session.GetString("user");
                if (string.IsNullOrEmpty(userSession)) // nếu k có session thì trả về lỗi
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var nguoiGui = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

                if (nguoiGui == null) // k tìm thấy user cũng báo lỗi
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                //Kiểm tra không được kết bạn với chính mình
                if (nguoiGui.ID == nguoiNhanId)
                {
                    return Json(new { success = false, message = "Không thể kết bạn với chính mình" });
                }

                //Kiểm tra người nhận có tồn tại không
                var nguoiNhan = await _context.Users
                    .FirstOrDefaultAsync(u => u.ID == nguoiNhanId); // tìm trog db theo id

                if (nguoiNhan == null) // không có báo lỗi
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại" });
                }

                // Kiểm tra đã gửi lời mời chưa
                var loiMoiDaTonTai = await _context.KetBans
                    .AnyAsync(k => // ktr xem có bất kỳ bản ghi nào thoả mãn đk không
                        (k.NguoiGuiId == nguoiGui.ID && k.NguoiNhanId == nguoiNhanId) || // nếu user a đã gửi cho user b
                        (k.NguoiGuiId == nguoiNhanId && k.NguoiNhanId == nguoiGui.ID));  //user b đã gửi cho user a

                if (loiMoiDaTonTai) // nếu tồm tại không gửi thêm
                {
                    return Json(new { success = false, message = "Đã tồn tại lời mời kết bạn" });
                }

                //Tạo lời mời kết bạn mới
                var ketBan = new KetBan
                {
                    NguoiGuiId = nguoiGui.ID,
                    NguoiNhanId = nguoiNhanId,
                    TrangThai = 0, // 0: Đang chờ xác nhận
                    NgayGui = DateTime.Now,
                    NgayPhanHoi = null
                };

                _context.KetBans.Add(ketBan); //thêm vào ds theo dõi của ef
                await _context.SaveChangesAsync(); // lưu thay đổi vào db

                // Trả về kết quả thành công
                return Json(new
                {
                    success = true, // thành công
                    message = "Đã gửi lời mời kết bạn", // thông báo
                    id = ketBan.Id // id bản ghi vừa tạo
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // ===== TRANG LỜI MỜI KẾT BẠN =====
        public async Task<IActionResult> LoiMoiKetBan()
        {
            // Lấy user hiện tại
            var userSession = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userSession))
                return RedirectToAction("DangNhap");

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

            if (currentUser == null)
                return RedirectToAction("DangNhap");

            // Lấy danh sách lời mời kết bạn (người khác gửi cho mình, trạng thái 0)
            var loiMois = await _context.KetBans
                .Include(k => k.NguoiGui)  // Lấy thông tin người gửi
                .Where(k => k.NguoiNhanId == currentUser.ID && k.TrangThai == 0)
                .OrderByDescending(k => k.NgayGui)
                .ToListAsync();

            return View(loiMois);
        }

        // ===== CHẤP NHẬN LỜI MỜI KẾT BẠN =====
        [HttpPost]
        public async Task<IActionResult> ChapNhanLoiMoi(int loiMoiId)
        {
            try
            {
                // Tìm lời mời
                var loiMoi = await _context.KetBans.FindAsync(loiMoiId);
                if (loiMoi == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lời mời" });
                }

                // Cập nhật trạng thái thành 1 (bạn bè)
                loiMoi.TrangThai = 1;
                loiMoi.NgayPhanHoi = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã chấp nhận lời mời kết bạn" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // ===== TỪ CHỐI LỜI MỜI KẾT BẠN =====
     
        [HttpPost]
        public async Task<IActionResult> TuChoiLoiMoi(int loiMoiId)
        {
            try
            {
                var loiMoi = await _context.KetBans.FindAsync(loiMoiId);
                if (loiMoi == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lời mời" });
                }

                // 👉 XOÁ HẲN BẢN GHI (không giữ lại)
                _context.KetBans.Remove(loiMoi);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã từ chối lời mời kết bạn" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        // ===== ĐẾM SỐ LỜI MỜI KẾT BẠN CHƯA ĐỌC =====
        [HttpGet]
        public async Task<IActionResult> DemLoiMoiKetBan()
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

                // Đếm số lời mời kết bạn (người khác gửi cho mình, trạng thái 0)
                var count = await _context.KetBans
                    .CountAsync(k => k.NguoiNhanId == currentUser.ID && k.TrangThai == 0);

                return Json(new { count = count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }
        // ===== HUỶ LỜI MỜI KẾT BẠN ĐÃ GỬI =====
        [HttpPost]
        public async Task<IActionResult> HuyLoiMoiKetBan(int nguoiNhanId)
        {
            try
            {
                // Lấy user hiện tại
                var userSession = HttpContext.Session.GetString("user");
                if (string.IsNullOrEmpty(userSession))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var nguoiGui = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

                if (nguoiGui == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                // Tìm lời mời đã gửi (trạng thái 0)
                var loiMoi = await _context.KetBans
                    .FirstOrDefaultAsync(k =>
                        k.NguoiGuiId == nguoiGui.ID &&
                        k.NguoiNhanId == nguoiNhanId &&
                        k.TrangThai == 0);

                if (loiMoi == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lời mời" });
                }

                // Xoá lời mời (hoặc có thể đánh dấu trạng thái 3 là "đã huỷ")
                _context.KetBans.Remove(loiMoi);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã huỷ lời mời kết bạn" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]  
        public async Task<IActionResult> HuyKetBan(int banId)  // banId là ID của người muốn huỷ
        {
            try  // Bắt mọi lỗi có thể xảy ra
            {
                // LẤY THÔNG TIN NGƯỜI DÙNG HIỆN TẠI ===
                var userSession = HttpContext.Session.GetString("user");  // Lấy email/sdt từ session
                if (string.IsNullOrEmpty(userSession))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);  // Tìm user trong DB theo email

                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                // TÌM MỐI QUAN HỆ BẠN BÈ GIỮA 2 NGƯỜI ===
                var ketBan = await _context.KetBans
                    .FirstOrDefaultAsync(k =>
                        ((k.NguoiGuiId == currentUser.ID && k.NguoiNhanId == banId) ||  // TH1: Mình gửi, bạn nhận
                         (k.NguoiGuiId == banId && k.NguoiNhanId == currentUser.ID)) && // TH2: Bạn gửi, mình nhận
                        k.TrangThai == 1); // Trạng thái 1 = bạn bè

                if (ketBan == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy mối quan hệ bạn bè" });
                }

                // XOÁ BẢN GHI KẾT BẠN ===
                _context.KetBans.Remove(ketBan);  // Xoá khỏi DbContext
                await _context.SaveChangesAsync();  // Lưu thay đổi xuống database

                //TRẢ VỀ KẾT QUẢ ===
                return Json(new { success = true, message = "Đã huỷ kết bạn" });
            }
            catch (Exception ex)  // Nếu có lỗi bất kỳ
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        // === TRANG TÌM KIẾM NGƯỜI DÙNG ===
        public async Task<IActionResult> TimKiem(string tuKhoa)
        {
            // Lấy user hiện tại
            var userSession = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userSession))
                return RedirectToAction("DangNhap");

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

            if (currentUser == null)
                return RedirectToAction("DangNhap");

            // Nếu không có từ khóa, trả về view rỗng
            if (string.IsNullOrEmpty(tuKhoa))
            {
                ViewBag.KetQua = new List<User>();
                return View();
            }
            
            // Tìm kiếm người dùng theo tên (không phân biệt hoa thường)
            var ketQua = await _context.Users
                .Where(u => u.Ten.Contains(tuKhoa) && u.ID != currentUser.ID)
                .Take(20) // Giới hạn 20 kết quả
                .ToListAsync();

            // Lấy thông tin kết bạn cho mỗi kết quả
            var ketQuaVoiTrangThai = new List<dynamic>();
            foreach (var user in ketQua)
            {
                bool daLaBanBe = await _context.KetBans
                    .AnyAsync(k =>
                        ((k.NguoiGuiId == currentUser.ID && k.NguoiNhanId == user.ID) ||
                         (k.NguoiGuiId == user.ID && k.NguoiNhanId == currentUser.ID)) &&
                        k.TrangThai == 1);

                bool daGuiLoiMoi = false;
                if (!daLaBanBe)
                {
                    daGuiLoiMoi = await _context.KetBans
                        .AnyAsync(k =>
                            k.NguoiGuiId == currentUser.ID &&
                            k.NguoiNhanId == user.ID &&
                            k.TrangThai == 0);
                }

                ketQuaVoiTrangThai.Add(new
                {
                    User = user,
                    DaLaBanBe = daLaBanBe,
                    DaGuiLoiMoi = daGuiLoiMoi
                });
            }

            ViewBag.KetQua = ketQuaVoiTrangThai;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.CurrentUserId = currentUser.ID;

            return View();
        }

    }
}

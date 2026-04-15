// BÙI ĐỨC HÀ - LOTUS
using LOHA.Models; // dùng các class trong thư mục model
using Microsoft.AspNetCore.Identity; // mã hoá mật khẩu
using LOHA.Services;
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
        public async Task<IActionResult> DangKy(User user, string XacNhanMatKhau)
        {
            // 1. Kiểm tra xác nhận mật khẩu
            if (user.Matkhau != XacNhanMatKhau)
            {
                ModelState.AddModelError("Matkhau", "Mật khẩu xác nhận không khớp");
            }

            if (ModelState.IsValid)
            {
                // 2. Kiểm tra email/sdt đã tồn tại chưa
                var existingUser = _context.Users.FirstOrDefault(u => u.EmailorSDT == user.EmailorSDT);
                if (existingUser != null)
                {
                    ModelState.AddModelError("EmailorSDT", "Email hoặc số điện thoại đã được sử dụng");
                    return View(user);
                }

                // 3. Tạo mã OTP 6 số ngẫu nhiên
                Random random = new Random();
                string maOTP = random.Next(100000, 999999).ToString();

                // 4. Lưu thông tin đăng ký tạm thời vào bảng XacThucEmail
                var xacThuc = new XacThucEmail
                {
                    Email = user.EmailorSDT,
                    MaOTP = maOTP,
                    Ten = user.Ten,
                    Matkhau = user.Matkhau,
                    Ngaysinh = user.Ngaysinh,
                    Gioitinh = user.Gioitinh,
                    ThoiGianTao = DateTime.Now,
                    ThoiGianHetHan = DateTime.Now.AddMinutes(5), // Hết hạn sau 5 phút
                    DaSuDung = false
                };

                _context.XacThucEmails.Add(xacThuc);
                await _context.SaveChangesAsync();

                // 5. Gửi email chứa mã OTP
                string subject = "LOHA - Xác thực tài khoản";
                string body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto;'>
            <div style='text-align: center; padding: 20px;'>
                <h2 style='color: #1E2A78;'>LOHA - Xác thực tài khoản</h2>
            </div>
            <div style='background: #F8FAFF; padding: 20px; border-radius: 10px;'>
                <p>Xin chào <strong>{user.Ten}</strong>,</p>
                <p>Cảm ơn bạn đã đăng ký tài khoản LOHA!</p>
                <p>Mã xác thực của bạn là:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <span style='background: linear-gradient(135deg, #1E2A78, #6C63FF); 
                                 color: white; 
                                 padding: 15px 40px; 
                                 font-size: 32px; 
                                 font-weight: bold; 
                                 letter-spacing: 10px;
                                 border-radius: 10px;'>
                        {maOTP}
                    </span>
                </div>
                <p style='color: #6B7280; font-size: 14px;'>
                    <i class='fa-regular fa-clock'></i> 
                    Mã này sẽ hết hạn sau 5 phút.
                </p>
                <p style='color: #6B7280; font-size: 14px;'>
                    Nếu bạn không đăng ký tài khoản LOHA, vui lòng bỏ qua email này.
                </p>
            </div>
            <div style='text-align: center; padding: 20px; color: #9CA3AF; font-size: 12px;'>
                © 2024 LOHA - BĐH LOTUS
            </div>
        </div>";

                var emailService = HttpContext.RequestServices.GetService<LOHA.Services.EmailService>();
                if (emailService != null)
                {
                    await emailService.SendEmailAsync(user.EmailorSDT, subject, body);
                }

                // 6. Chuyển đến trang xác thực OTP
                return RedirectToAction("XacThucDangKy", new { email = user.EmailorSDT });
            }

            return View(user);
        }
        // ===== TRANG XÁC THỰC OTP KHI ĐĂNG KÝ =====
        [HttpGet]
        public IActionResult XacThucDangKy(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("DangKy");
            }

            ViewBag.Email = email;
            return View();
        }
        // ===== XÁC NHẬN OTP VÀ TẠO TÀI KHOẢN =====
        [HttpPost]
        public async Task<IActionResult> XacNhanDangKy(string email, string maOTP)
        {
            // 1. Kiểm tra đầu vào
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(maOTP))
            {
                TempData["Loi"] = "Vui lòng nhập mã OTP";
                return RedirectToAction("XacThucDangKy", new { email = email });
            }

            // 2. Tìm bản ghi xác thực trong database
            var xacThuc = await _context.XacThucEmails
                .Where(x => x.Email == email && x.MaOTP == maOTP && !x.DaSuDung)
                .OrderByDescending(x => x.ThoiGianTao)
                .FirstOrDefaultAsync();

            // 3. Kiểm tra OTP có tồn tại không
            if (xacThuc == null)
            {
                TempData["Loi"] = "Mã OTP không đúng hoặc đã được sử dụng";
                return RedirectToAction("XacThucDangKy", new { email = email });
            }

            // 4. Kiểm tra OTP còn hạn không (5 phút)
            if (xacThuc.ThoiGianHetHan < DateTime.Now)
            {
                TempData["Loi"] = "Mã OTP đã hết hạn. Vui lòng đăng ký lại";
                return RedirectToAction("DangKy");
            }

            // 5. Kiểm tra email đã tồn tại chưa (phòng trường hợp đăng ký trùng khi chờ xác thực)
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.EmailorSDT == email);
            if (existingUser != null)
            {
                TempData["Loi"] = "Email này đã được đăng ký";
                return RedirectToAction("DangKy");
            }

            // 6. Tạo user mới từ thông tin đã lưu
            var user = new User
            {
                EmailorSDT = xacThuc.Email,
                Ten = xacThuc.Ten,
                Matkhau = xacThuc.Matkhau,
                Ngaysinh = xacThuc.Ngaysinh,
                Gioitinh = xacThuc.Gioitinh,
                Ngaytao = DateTime.Now,
                NgayCapNhatTen = null,
                NgayCapNhatNgaySinh = null
            };

            _context.Users.Add(user);

            // 7. Đánh dấu OTP đã sử dụng
            xacThuc.DaSuDung = true;

            await _context.SaveChangesAsync();

            // 8. Thông báo thành công và chuyển về trang đăng nhập
            TempData["DangKyThanhCong"] = "true";
            TempData["ThongBao"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";

            return RedirectToAction("DangNhap");
        }
        // ===== GỬI LẠI MÃ XÁC THỰC =====
        [HttpGet]
        public async Task<IActionResult> GuiLaiMaXacThuc(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("DangKy");
            }

            // Tìm bản ghi xác thực chưa sử dụng
            var xacThuc = await _context.XacThucEmails
                .Where(x => x.Email == email && !x.DaSuDung)
                .OrderByDescending(x => x.ThoiGianTao)
                .FirstOrDefaultAsync();

            if (xacThuc == null)
            {
                TempData["Loi"] = "Không tìm thấy yêu cầu xác thực. Vui lòng đăng ký lại";
                return RedirectToAction("DangKy");
            }

            // Tạo mã OTP mới
            Random random = new Random();
            string maOTPMoi = random.Next(100000, 999999).ToString();

            // Cập nhật mã OTP và thời gian hết hạn
            xacThuc.MaOTP = maOTPMoi;
            xacThuc.ThoiGianTao = DateTime.Now;
            xacThuc.ThoiGianHetHan = DateTime.Now.AddMinutes(5);
            await _context.SaveChangesAsync();

            // Gửi lại email
            string subject = "LOHA - Mã xác thực mới";
            string body = $@"
    <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto;'>
        <div style='text-align: center; padding: 20px;'>
            <h2 style='color: #1E2A78;'>LOHA - Mã xác thực mới</h2>
        </div>
        <div style='background: #F8FAFF; padding: 20px; border-radius: 10px;'>
            <p>Xin chào <strong>{xacThuc.Ten}</strong>,</p>
            <p>Bạn đã yêu cầu gửi lại mã xác thực cho tài khoản LOHA.</p>
            <p>Mã xác thực mới của bạn là:</p>
            <div style='text-align: center; margin: 30px 0;'>
                <span style='background: linear-gradient(135deg, #1E2A78, #6C63FF); 
                             color: white; 
                             padding: 15px 40px; 
                             font-size: 32px; 
                             font-weight: bold; 
                             letter-spacing: 10px;
                             border-radius: 10px;'>
                    {maOTPMoi}
                </span>
            </div>
            <p style='color: #6B7280; font-size: 14px;'>
                <i class='fa-regular fa-clock'></i> 
                Mã này sẽ hết hạn sau 5 phút.
            </p>
        </div>
        <div style='text-align: center; padding: 20px; color: #9CA3AF; font-size: 12px;'>
            © 2024 LOHA - BĐH LOTUS
        </div>
    </div>";

            var emailService = HttpContext.RequestServices.GetService<LOHA.Services.EmailService>();
            if (emailService != null)
            {
                await emailService.SendEmailAsync(email, subject, body);
            }

            TempData["ThongBao"] = "Mã xác thực mới đã được gửi đến email của bạn";
            return RedirectToAction("XacThucDangKy", new { email = email });
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
            // Thêm kiểm tra tài khoản bị khóa
            
            if (user == null || user.Matkhau != model.Matkhau) // So sánh trực tiếp
            {
                ModelState.AddModelError("", "Sai email hoặc mật khẩu");
                return View(model);
            }
            if (!user.TrangThai)
            {
                TempData["TaiKhoanBiKhoa"] = "true";
                return RedirectToAction("DangNhap");
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

                // Kiểm tra đã gửi lời mời chưa (CHỈ KHI CHƯA LÀ BẠN BÈ)
                if (!daLaBanBe)
                {
                    daGuiLoiMoi = _context.KetBans.Any(k =>
                        k.NguoiGuiId == currentUser.ID &&
                        k.NguoiNhanId == user.ID &&
                        k.TrangThai == 0);
                }

                // Nếu đã là bạn bè, lấy nhóm mà currentUser đã xếp cho user này
                if (daLaBanBe)
                {
                    var ketBan = _context.KetBans.FirstOrDefault(k =>
                        ((k.NguoiGuiId == currentUser.ID && k.NguoiNhanId == user.ID) ||
                         (k.NguoiGuiId == user.ID && k.NguoiNhanId == currentUser.ID)) &&
                        k.TrangThai == 1);

                    if (ketBan != null)
                    {
                        // Xác định nhóm dựa vào việc currentUser là người gửi hay người nhận
                        if (ketBan.NguoiGuiId == currentUser.ID)
                            ViewBag.NhomCuaToi = ketBan.NhomNguoiGui ?? "Bạn bè";
                        else
                            ViewBag.NhomCuaToi = ketBan.NhomNguoiNhan ?? "Bạn bè";
                    }
                    else
                    {
                        ViewBag.NhomCuaToi = "Bạn bè";
                    }
                }
            }

            // Đếm số bạn bè của user này
            soBanBe = _context.KetBans.Count(k =>
                (k.NguoiGuiId == user.ID || k.NguoiNhanId == user.ID) &&
                k.TrangThai == 1);

            // Truyền dữ liệu sang View qua ViewBag
            ViewBag.Baiviets = baiviets;
            ViewBag.Thichs = thichs;
            ViewBag.CurrentUserId = currentUser.ID;
            ViewBag.DaLaBanBe = daLaBanBe;
            ViewBag.DaGuiLoiMoi = daGuiLoiMoi;
            ViewBag.SoBanBe = soBanBe;

            // ===== LẤY THAM SỐ LỌC NHÓM TỪ QUERY STRING =====
            string nhomLoc = HttpContext.Request.Query["nhom"].ToString();
            ViewBag.NhomDangChon = nhomLoc; // Để view biết đang lọc nhóm nào

            // Lấy danh sách bạn bè
            var danhSachBanBe = new List<User>();
            var danhSachNhom = new List<string>(); // Lưu nhóm tương ứng với từng bạn

            // Lấy tất cả quan hệ bạn bè của user này
            var cacKetBan = await _context.KetBans
                .Where(k => (k.NguoiGuiId == user.ID || k.NguoiNhanId == user.ID) && k.TrangThai == 1)
                .OrderByDescending(k => k.NgayPhanHoi ?? k.NgayGui) // Mới nhất lên đầu
                .ToListAsync();

            ViewBag.NgayKetBan = new Dictionary<int, DateTime>();
            ViewBag.NhomCuaBan = new Dictionary<int, string>(); // Lưu nhóm của từng bạn

            foreach (var ketBan in cacKetBan)
            {
                User? ban = null;
                string nhomCuaBanBe = "Bạn bè";

                if (ketBan.NguoiGuiId == user.ID)
                {
                    ban = await _context.Users.FindAsync(ketBan.NguoiNhanId);
                    // Nhóm mà user xếp cho người này
                    nhomCuaBanBe = ketBan.NhomNguoiGui ?? "Bạn bè";
                }
                else
                {
                    ban = await _context.Users.FindAsync(ketBan.NguoiGuiId);
                    // Nhóm mà user xếp cho người này
                    nhomCuaBanBe = ketBan.NhomNguoiNhan ?? "Bạn bè";
                }

                if (ban != null)
                {
                    // Lọc theo nhóm nếu có tham số
                    if (!string.IsNullOrEmpty(nhomLoc) && nhomLoc != "TatCa")
                    {
                        if (nhomCuaBanBe != nhomLoc)
                            continue; // Bỏ qua nếu không đúng nhóm
                    }

                    ViewBag.NgayKetBan[ban.ID] = ketBan.NgayPhanHoi ?? ketBan.NgayGui;
                    ViewBag.NhomCuaBan[ban.ID] = nhomCuaBanBe;
                    danhSachBanBe.Add(ban);
                }
            }

            ViewBag.DanhSachBanBe = danhSachBanBe;

            return View(user);
        }
        // ===== CHỈNH SỬA TRANG CÁ NHÂN =====

        [HttpPost]
        public async Task<IActionResult> ChinhSua(
    string? Ten,
    DateTime? Ngaysinh,
    string? MatKhauCu,
    string? MatKhauMoi,
    string? XacNhanMatKhau,
    string? confirm,
    string? activeTab)
        {
            var userSession = HttpContext.Session.GetString("user");

            if (string.IsNullOrEmpty(userSession))
                return RedirectToAction("DangNhap");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

            if (user == null)
                return RedirectToAction("DangNhap");

            var now = DateTime.Now;

            // ===== KIỂM TRA MẬT KHẨU =====
            if (string.IsNullOrWhiteSpace(MatKhauCu))
                ModelState.AddModelError("MatKhauCu", "Phải nhập mật khẩu hiện tại");
            else if (MatKhauCu != user.Matkhau)
                ModelState.AddModelError("MatKhauCu", "Mật khẩu không đúng");

            // ===== VALIDATE TÊN =====
            if (string.IsNullOrWhiteSpace(Ten))
            {
                ModelState.AddModelError("Ten", "Tên không được để trống");
            }
            else if (Ten != user.Ten)
            {
                // Chuẩn hóa
                Ten = Ten?.Trim();
                var words = Ten.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // 1. Kiểm tra số từ (2 - 5)
                if (words.Length < 2 || words.Length > 5)
                {
                    ModelState.AddModelError("Ten", "Tên phải từ 2 đến 5 từ");
                }
                // 2. Kiểm tra từng ký tự
                else if (Ten.Any(c => !char.IsLetter(c) && c != ' '))
                {
                    ModelState.AddModelError("Ten", "Tên không được chứa số hoặc ký tự đặc biệt");
                }
                if (Ten.Length > 40)
                    ModelState.AddModelError("Ten", "Tên không được quá 40 ký tự");

                else if (user.NgayCapNhatTen != null &&
                    (now - user.NgayCapNhatTen.Value).TotalDays < 60)
                    ModelState.AddModelError("Ten", "Bạn chỉ được đổi tên sau 60 ngày");
            }

            // ===== VALIDATE NGÀY SINH =====
            if (Ngaysinh != null && Ngaysinh != user.Ngaysinh)
            {
                if (Ngaysinh > now.AddYears(-10) || Ngaysinh < now.AddYears(-100))
                    ModelState.AddModelError("Ngaysinh", "Tuổi phải từ 10 đến 100");

                else if (user.NgayCapNhatNgaySinh != null &&
                    (now - user.NgayCapNhatNgaySinh.Value).TotalDays < 60)
                    ModelState.AddModelError("Ngaysinh", "Bạn chỉ được đổi ngày sinh sau 60 ngày");
            }

            // ===== VALIDATE MẬT KHẨU =====
            bool coNhapMK = !string.IsNullOrWhiteSpace(MatKhauMoi) ||
                            !string.IsNullOrWhiteSpace(XacNhanMatKhau);

            if (coNhapMK)
            {
                if (string.IsNullOrWhiteSpace(MatKhauMoi))
                    ModelState.AddModelError("MatKhauMoi", "Phải nhập mật khẩu mới");
                else if (MatKhauMoi.Length < 6)
                    ModelState.AddModelError("MatKhauMoi", "Mật khẩu phải tối thiểu 6 ký tự");
                else if (string.IsNullOrWhiteSpace(XacNhanMatKhau))
                    ModelState.AddModelError("XacNhanMatKhau", "Phải xác nhận mật khẩu");
                else if (MatKhauMoi != XacNhanMatKhau)
                    ModelState.AddModelError("XacNhanMatKhau", "Mật khẩu xác nhận không khớp");
            }
            // Xóa lỗi không liên quan cho từng mục
            if (activeTab != "matkhau")
            {
                ModelState.Remove("MatKhauMoi");
                ModelState.Remove("XacNhanMatKhau");
            }

            if (activeTab != "ten")
            {
                ModelState.Remove("Ten");
            }

            if (activeTab != "ngaysinh")
            {
                ModelState.Remove("Ngaysinh");
            }
            // ===== NẾU CÓ LỖI =====
            if (!ModelState.IsValid)
            {
                ViewBag.ActiveTab = activeTab;
                return View(user);
            }

            // ===== CHƯA CONFIRM → MỞ MODAL =====
            if (confirm != "true")
            {

                ViewBag.ShowConfirm = true;
                ViewBag.ActiveTab = activeTab;

                ViewBag.Ten = Ten;
                ViewBag.Ngaysinh = Ngaysinh;
                ViewBag.MatKhauCu = MatKhauCu;
                ViewBag.MatKhauMoi = MatKhauMoi;
                ViewBag.XacNhanMatKhau = XacNhanMatKhau;

                return View(user);
            }

            // ===== UPDATE THẬT =====
            if (!string.IsNullOrWhiteSpace(Ten) && Ten != user.Ten)
            {
                user.Ten = Ten;
                user.NgayCapNhatTen = now;
            }

            if (Ngaysinh != null && Ngaysinh != user.Ngaysinh)
            {
                user.Ngaysinh = Ngaysinh;
                user.NgayCapNhatNgaySinh = now;
            }

            if (coNhapMK)
            {
                user.Matkhau = MatKhauMoi;
            }

            await _context.SaveChangesAsync();

            ViewBag.Success = "Cập nhật thành công!";
            ViewBag.ActiveTab = activeTab;

            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> ChinhSua(string? tab = null)
        {
            var userSession = HttpContext.Session.GetString("user");

            if (string.IsNullOrEmpty(userSession))
                return RedirectToAction("DangNhap");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

            if (user == null)
                return RedirectToAction("DangNhap");

            var now = DateTime.Now;

            // ===== CHECK 60 NGÀY TÊN =====
            if (user.NgayCapNhatTen != null &&
                (now - user.NgayCapNhatTen.Value).TotalDays < 60)
            {
                ViewBag.BlockTen = true;
                ViewBag.MsgTen = "Bạn chỉ được đổi tên sau 60 ngày";
            }

            // ===== CHECK 60 NGÀY NGÀY SINH =====
            if (user.NgayCapNhatNgaySinh != null &&
                (now - user.NgayCapNhatNgaySinh.Value).TotalDays < 60)
            {
                ViewBag.BlockNgaySinh = true;
                ViewBag.MsgNgaySinh = "Bạn chỉ được đổi ngày sinh sau 60 ngày";
            }

            ViewBag.ActiveTab = tab;

            return View(user);
        }

        // ===== GỬI LỜI MỜI KẾT BẠN (CÓ CHỌN NHÓM) =====
        [HttpPost]
        public async Task<IActionResult> GuiLoiMoiKetBan(int nguoiNhanId, string nhom = "Bạn bè")
        {
            try
            {
                // 1. Lấy thông tin user đang đăng nhập
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

                // 2. Kiểm tra không được kết bạn với chính mình
                if (nguoiGui.ID == nguoiNhanId)
                {
                    return Json(new { success = false, message = "Không thể kết bạn với chính mình" });
                }

                // 3. Kiểm tra người nhận có tồn tại không
                var nguoiNhan = await _context.Users
                    .FirstOrDefaultAsync(u => u.ID == nguoiNhanId);

                if (nguoiNhan == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại" });
                }

                // 4. Kiểm tra đã gửi lời mời chưa hoặc đã là bạn bè chưa
                var ketBanCu = await _context.KetBans
                    .FirstOrDefaultAsync(k =>
                        (k.NguoiGuiId == nguoiGui.ID && k.NguoiNhanId == nguoiNhanId) ||
                        (k.NguoiGuiId == nguoiNhanId && k.NguoiNhanId == nguoiGui.ID));

                if (ketBanCu != null)
                {
                    if (ketBanCu.TrangThai == 0)
                        return Json(new { success = false, message = "Đã gửi lời mời trước đó" });
                    if (ketBanCu.TrangThai == 1)
                        return Json(new { success = false, message = "Đã là bạn bè" });
                }

                // 5. Tạo lời mời kết bạn mới (CÓ THÊM NHÓM)
                var ketBan = new KetBan
                {
                    NguoiGuiId = nguoiGui.ID,
                    NguoiNhanId = nguoiNhanId,
                    TrangThai = 0, // 0: Đang chờ xác nhận
                    NgayGui = DateTime.Now,
                    NgayPhanHoi = null,
                    NhomNguoiGui = nhom // ← THÊM NHÓM NGƯỜI GỬI CHỌN
                };

                _context.KetBans.Add(ketBan);
                await _context.SaveChangesAsync();

                // 6. Trả về kết quả thành công
                return Json(new
                {
                    success = true,
                    message = "Đã gửi lời mời kết bạn",
                    nhom = nhom
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

        // ===== CHẤP NHẬN LỜI MỜI KẾT BẠN (CÓ CHỌN NHÓM) =====
        [HttpPost]
        public async Task<IActionResult> ChapNhanLoiMoi(int loiMoiId, string nhom = "Bạn bè")
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

                // Lưu nhóm mà người nhận (người chấp nhận) chọn
                loiMoi.NhomNguoiNhan = nhom;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã chấp nhận lời mời kết bạn", nhom = nhom });
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
                // Kiểm tra đã gửi lời mời chưa (trạng thái 0: đang chờ)
                if (!daLaBanBe)
                {
                    daGuiLoiMoi = _context.KetBans.Any(k =>
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
        // ===== TRANG QUÊN MẬT KHẨU =====
        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }
        // ===== GỬI EMAIL ĐẶT LẠI MẬT KHẨU =====
        [HttpPost]
        public async Task<IActionResult> GuiEmailDatLaiMatKhau(string email)
        {
            // 1. Kiểm tra email có được nhập không
            if (string.IsNullOrEmpty(email))
            {
                TempData["Loi"] = "Vui lòng nhập email";
                return RedirectToAction("QuenMatKhau");
            }

            // 2. Kiểm tra email có tồn tại trong database không
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailorSDT == email);
            if (user == null)
            {
                TempData["Loi"] = "Email không tồn tại trong hệ thống";
                return RedirectToAction("QuenMatKhau");
            }

            // 3. Tạo mã OTP 6 số ngẫu nhiên
            Random random = new Random();
            string maOTP = random.Next(100000, 999999).ToString(); // Sinh số từ 100000 đến 999999

            // 4. Lưu thông tin vào bảng DatLaiMatKhau
            var datLai = new DatLaiMatKhau
            {
                Email = email,
                MaOTP = maOTP, // ← ĐÚNG: Dùng MaOTP thay vì Token
                ThoiGianTao = DateTime.Now,
                ThoiGianHetHan = DateTime.Now.AddMinutes(5), // Hết hạn sau 5 phút (phù hợp với OTP)
                DaSuDung = false
            };

            _context.DatLaiMatKhau.Add(datLai);
            await _context.SaveChangesAsync();

            // 5. Tạo link đặt lại mật khẩu
            // Lấy domain hiện tại (http://localhost:xxxx)
            var request = HttpContext.Request;
            var domain = $"{request.Scheme}://{request.Host}";

            // 6. Tạo nội dung email
            // 6. Tạo nội dung email (hiển thị mã OTP 6 số)
            string subject = "LOHA - Mã xác nhận đặt lại mật khẩu";
            string body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto;'>
                <div style='text-align: center; padding: 20px;'>
                    <h2 style='color: #1E2A78;'>LOHA - Đặt lại mật khẩu</h2>
                </div>
                <div style='background: #F8FAFF; padding: 20px; border-radius: 10px;'>
                    <p>Xin chào <strong>{user.Ten}</strong>,</p>
                    <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản LOHA của mình.</p>
                    <p>Mã xác nhận của bạn là:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <span style='background: linear-gradient(135deg, #1E2A78, #6C63FF); 
                                     color: white; 
                                     padding: 15px 40px; 
                                     font-size: 32px; 
                                     font-weight: bold; 
                                     letter-spacing: 10px;
                                     border-radius: 10px;'>
                            {maOTP}
                        </span>
                    </div>
                    <p style='color: #6B7280; font-size: 14px;'>
                        <i class='fa-regular fa-clock'></i> 
                        Mã này sẽ hết hạn sau 5 phút.
                    </p>
                    <p style='color: #6B7280; font-size: 14px;'>
                        Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
                    </p>
                </div>
                <div style='text-align: center; padding: 20px; color: #9CA3AF; font-size: 12px;'>
                    © 2024 LOHA - BĐH LOTUS
                </div>
            </div>";

            // 7. Gửi email
            // Lấy EmailService từ HttpContext (đã đăng ký trong Program.cs)
            var emailService = HttpContext.RequestServices.GetService<LOHA.Services.EmailService>();

            if (emailService != null)
            {
                bool ketQua = await emailService.SendEmailAsync(email, subject, body);

                if (ketQua)
                {
                    TempData["ThongBao"] = "Link đặt lại mật khẩu đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư!";
                }
                else
                {
                    TempData["Loi"] = "Không thể gửi email. Vui lòng thử lại sau.";
                }
            }
            else
            {
                TempData["Loi"] = "Lỗi hệ thống. Vui lòng thử lại sau.";
            }

            return RedirectToAction("XacNhanOTP", new { email = email });
        }
        // ===== TRANG NHẬP MÃ OTP =====
        [HttpGet]
        public IActionResult XacNhanOTP(string email)
        {
            // Kiểm tra email có được truyền vào không
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("QuenMatKhau");
            }

            // Truyền email sang View để dùng trong form
            ViewBag.Email = email;

            return View();
        }
        // ===== XỬ LÝ ĐẶT LẠI MẬT KHẨU =====
        [HttpPost]
        public async Task<IActionResult> DatLaiMatKhau(string email, string maOTP, string matKhauMoi, string xacNhanMatKhau)
        {
            // 1. Kiểm tra các trường có được nhập không
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(maOTP) ||
                string.IsNullOrEmpty(matKhauMoi) || string.IsNullOrEmpty(xacNhanMatKhau))
            {
                TempData["Loi"] = "Vui lòng nhập đầy đủ thông tin";
                return RedirectToAction("XacNhanOTP", new { email = email });
            }

            // 2. Kiểm tra mật khẩu mới và xác nhận có khớp không
            if (matKhauMoi != xacNhanMatKhau)
            {
                TempData["Loi"] = "Mật khẩu xác nhận không khớp";
                return RedirectToAction("XacNhanOTP", new { email = email });
            }

            // 3. Kiểm tra độ dài mật khẩu (tối thiểu 6 ký tự)
            if (matKhauMoi.Length < 6)
            {
                TempData["Loi"] = "Mật khẩu phải có ít nhất 6 ký tự";
                return RedirectToAction("XacNhanOTP", new { email = email });
            }

            // 4. Kiểm tra mật khẩu có cả chữ và số không
            bool coChu = matKhauMoi.Any(char.IsLetter);
            bool coSo = matKhauMoi.Any(char.IsDigit);
            if (!coChu || !coSo)
            {
                TempData["Loi"] = "Mật khẩu phải bao gồm cả chữ và số";
                return RedirectToAction("XacNhanOTP", new { email = email });
            }

            // 5. Tìm bản ghi OTP trong database
            var datLai = await _context.DatLaiMatKhau
                .Where(d => d.Email == email && d.MaOTP == maOTP && !d.DaSuDung)
                .OrderByDescending(d => d.ThoiGianTao) // Lấy bản ghi mới nhất
                .FirstOrDefaultAsync();

            // 6. Kiểm tra OTP có tồn tại không
            if (datLai == null)
            {
                TempData["Loi"] = "Mã OTP không đúng hoặc đã được sử dụng";
                return RedirectToAction("XacNhanOTP", new { email = email });
            }

            // 7. Kiểm tra OTP còn hạn không (5 phút)
            if (datLai.ThoiGianHetHan < DateTime.Now)
            {
                TempData["Loi"] = "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới";
                return RedirectToAction("QuenMatKhau");
            }

            // 8. Tìm user theo email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailorSDT == email);
            if (user == null)
            {
                TempData["Loi"] = "Không tìm thấy tài khoản";
                return RedirectToAction("QuenMatKhau");
            }

            // 9. Cập nhật mật khẩu mới
            user.Matkhau = matKhauMoi;

            // 10. Đánh dấu OTP đã sử dụng
            datLai.DaSuDung = true;

            await _context.SaveChangesAsync();

            // 11. Thông báo thành công và chuyển về trang đăng nhập
            TempData["ThongBao"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.";

            return RedirectToAction("DangNhap");
        }


        // Chức năng Thay đổi Avatar và Ảnh nền

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Lấy email/sdt từ session
                var userEmail = HttpContext.Session.GetString("user");
                if (string.IsNullOrEmpty(userEmail))
                    return Json(new { success = false, message = "Chưa đăng nhập" });

                // Tìm user bằng email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailorSDT == userEmail);
                if (user == null)
                    return Json(new { success = false, message = "Không tìm thấy user" });

                // ❌ XÓA ẢNH CŨ
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Avatar.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // ✅ LƯU ẢNH MỚI
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/avatar");

                // 👉 THÊM 3 DÒNG NÀY: Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var path = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                user.Avatar = "/images/avatar/" + fileName;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UploadCover(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Lấy email/sdt từ session
                var userEmail = HttpContext.Session.GetString("user");
                if (string.IsNullOrEmpty(userEmail))
                    return Json(new { success = false, message = "Chưa đăng nhập" });

                // Tìm user bằng email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailorSDT == userEmail);
                if (user == null)
                    return Json(new { success = false, message = "Không tìm thấy user" });

                // ❌ XÓA ẢNH CŨ
                if (!string.IsNullOrEmpty(user.AnhNen))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AnhNen.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // ✅ LƯU ẢNH MỚI
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cover");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var path = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                user.AnhNen = "/images/cover/" + fileName;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }
        // ===== GỬI BÁO CÁO BÀI VIẾT =====
        [HttpPost]
        public async Task<IActionResult> BaoCaoBaiViet(int baiVietId, string lyDo)
        {
            try
            {
                // Lấy user hiện tại
                var userSession = HttpContext.Session.GetString("user");
                if (string.IsNullOrEmpty(userSession))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.EmailorSDT == userSession);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy user" });
                }

                // Kiểm tra bài viết tồn tại
                var baiViet = await _context.Baiviets.FindAsync(baiVietId);
                if (baiViet == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài viết" });
                }

                // Kiểm tra đã báo cáo chưa (tránh spam)
                var daBaoCao = await _context.BaoCaoBaiViets
                    .AnyAsync(b => b.NguoiBaoCaoId == currentUser.ID && b.BaiVietId == baiVietId && b.TrangThai == 0);

                if (daBaoCao)
                {
                    return Json(new { success = false, message = "Bạn đã báo cáo bài viết này rồi" });
                }

                // Tạo báo cáo mới
                var baoCao = new BaoCaoBaiViet
                {
                    NguoiBaoCaoId = currentUser.ID,
                    BaiVietId = baiVietId,
                    LyDo = lyDo,
                    ThoiGian = DateTime.Now,
                    TrangThai = 0 // Chờ xử lý
                };

                _context.BaoCaoBaiViets.Add(baoCao);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã gửi báo cáo" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        // ===== GỬI BÁO CÁO NGƯỜI DÙNG (GIỚI HẠN 12 TIẾNG) =====
        [HttpPost]
        public async Task<IActionResult> BaoCaoNguoiDung(int nguoiBiBaoCaoId, string lyDo)
        {
            try
            {
                // 1. Lấy user hiện tại
                var userSession = HttpContext.Session.GetString("user");
                if (string.IsNullOrEmpty(userSession))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.EmailorSDT == userSession);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy user" });
                }

                // 2. Kiểm tra không tự báo cáo chính mình
                if (currentUser.ID == nguoiBiBaoCaoId)
                {
                    return Json(new { success = false, message = "Không thể báo cáo chính mình" });
                }

                // 3. Kiểm tra người bị báo cáo có tồn tại không
                var nguoiBiBaoCao = await _context.Users.FindAsync(nguoiBiBaoCaoId);
                if (nguoiBiBaoCao == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại" });
                }

                // 4. Kiểm tra giới hạn 12 tiếng
                var gioiHan12Tieng = DateTime.Now.AddHours(-12);
                var daBaoCaoGanDay = await _context.BaoCaoNguoiDungs
                    .AnyAsync(b => b.NguoiBaoCaoId == currentUser.ID
                                && b.NguoiBiBaoCaoId == nguoiBiBaoCaoId
                                && b.ThoiGian >= gioiHan12Tieng);

                if (daBaoCaoGanDay)
                {
                    return Json(new { success = false, message = "Bạn đã báo cáo người dùng này trong 12 giờ qua. Vui lòng đợi thêm!" });
                }

                // 5. Tạo báo cáo mới
                var baoCao = new BaoCaoNguoiDung
                {
                    NguoiBaoCaoId = currentUser.ID,
                    NguoiBiBaoCaoId = nguoiBiBaoCaoId,
                    LyDo = lyDo,
                    ThoiGian = DateTime.Now,
                    TrangThai = 0 // Chờ xử lý
                };

                _context.BaoCaoNguoiDungs.Add(baoCao);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã gửi báo cáo. Cảm ơn bạn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

    }
}

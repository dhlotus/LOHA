using LOHA.Hubs;
using LOHA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace LOHA.Controllers
{
    public class BaivietController : Controller
    {
        private readonly AppDbContext _context;

        public BaivietController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            // 1. LẤY USER HIỆN TẠI TỪ SESSION
            var userSession = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userSession))
            {
                return RedirectToAction("DangNhap", "User");
            }

            var userHT = _context.Users.FirstOrDefault(u => u.EmailorSDT == userSession);
            if (userHT == null)
            {
                return RedirectToAction("DangNhap", "User");
            }

            int currentUserId = userHT.ID;

            // 2. LẤY DANH SÁCH ID BẠN BÈ (Chỉ những người có TrangThai == 1)
            // Tìm trong bảng KetBans: 
            // Nếu mình là NguoiGui thì lấy ID NguoiNhan. Nếu mình là NguoiNhan thì lấy ID NguoiGui.
            var friendIds = _context.KetBans
                .Where(kb => (kb.NguoiGuiId == currentUserId || kb.NguoiNhanId == currentUserId) && kb.TrangThai == 1)
                .Select(kb => kb.NguoiGuiId == currentUserId ? kb.NguoiNhanId : kb.NguoiGuiId)
                .ToList();

            // Thêm chính mình vào danh sách để thấy cả bài mình đăng
            friendIds.Add(currentUserId);

            // 3. TRUY VẤN BÀI VIẾT (Lọc theo danh sách friendIds)
            var baiviets = _context.Baiviets
                .Include(b => b.User)
                .Include(b => b.Binhluans)
                    .ThenInclude(bl => bl.User)
                .Include(b => b.Thichs)
                .Where(b => friendIds.Contains(b.UserId))
                .OrderByDescending(b => b.Ngaydang)
                .ToList();
            List<int> thichs = new List<int>();
            if (!string.IsNullOrEmpty(userSession))
            {
                var currentUser = _context.Users.FirstOrDefault(u => u.EmailorSDT == userSession);
                if (currentUser != null)
                {
                    thichs = _context.Thichs
                        .Where(t => t.UserId == currentUser.ID)
                        .Select(t => t.BaivietId)
                        .ToList();
                }
            }
            ViewBag.Thichs = thichs;
            ViewBag.CurrentUserId = currentUserId;

            if (userHT != null)
            {
                ViewBag.CurrentUserAvatar = string.IsNullOrEmpty(userHT.Avatar)
                    ? "/images/default.png"
                    : userHT.Avatar;
            }
            else
            {
                ViewBag.CurrentUserAvatar = "/images/default.png";
            }

            return View(baiviets);
        }
        public IActionResult Taobaiviet()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Taobaiviet(Baiviet bv, IFormFile AnhFile)
        {
            var user = HttpContext.Session.GetString("user");

            if (user == null)
            {
                return RedirectToAction("DangNhap", "User");
            }

            var userDB = _context.Users.FirstOrDefault(x => x.EmailorSDT == user);

            if (userDB == null)
            {
                return RedirectToAction("DangNhap", "User");
            }

            bv.UserId = userDB.ID;

            if (AnhFile != null)
            {
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(AnhFile.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/baiviet", filename);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    AnhFile.CopyTo(stream);
                }
                bv.Anh = filename;
            }

            bv.Ngaydang = DateTime.Now;

            _context.Baiviets.Add(bv);
            _context.SaveChanges();

            // --- SỬA ĐÚNG DÒNG NÀY ĐỂ Ở LẠI TRANG CHỦ ---
            return RedirectToAction("Index", "Baiviet");
        }


        [HttpPost]
        public async Task<IActionResult> ThemBinhLuan(int baivietId, string noidung, string anchor)
        {
            var userSession = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userSession))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            var user = _context.Users.FirstOrDefault(u => u.EmailorSDT == userSession);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy user" });
            }

            // Tìm bài viết để lấy chủ bài
            var baiViet = _context.Baiviets.FirstOrDefault(b => b.Id == baivietId);
            if (baiViet == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài viết" });
            }

            // Tạo bình luận mới
            var binhluan = new Binhluan
            {
                BaivietId = baivietId,
                UserId = user.ID,
                Noidung = noidung,
                Ngaydang = DateTime.Now
            };

            _context.Binhluans.Add(binhluan);
            _context.SaveChanges();

            // ===== CẬP NHẬT THÔNG BÁO (DÙNG SỐ LƯỢNG THỰC TẾ) =====
            if (user.ID != baiViet.UserId) // Không tự comment bài mình
            {
                // Đếm tổng số bình luận thực tế (không tính comment của chủ bài)
                var tongSoComment = _context.Binhluans
                    .Count(bl => bl.BaivietId == baivietId && bl.UserId != baiViet.UserId);

                // Tìm thông báo comment cho bài viết này
                var thongBao = _context.ThongBaos
                    .FirstOrDefault(t => t.UserId == baiViet.UserId
                                      && t.BaiVietId == baivietId
                                      && t.Loai == "comment");

                if (tongSoComment > 0)
                {
                    if (thongBao != null)
                    {
                        // Cập nhật số lượng = tổng comment thực tế
                        thongBao.SoLuong = tongSoComment;
                        thongBao.DaDoc = false;
                        thongBao.ThoiGianCapNhat = DateTime.Now;
                    }
                    else
                    {
                        // Tạo mới với số lượng = tổng comment thực tế
                        _context.ThongBaos.Add(new ThongBao
                        {
                            UserId = baiViet.UserId,
                            BaiVietId = baivietId,
                            Loai = "comment",
                            SoLuong = tongSoComment,
                            DaDoc = false,
                            ThoiGianTao = DateTime.Now,
                            ThoiGianCapNhat = DateTime.Now
                        });
                    }
                    _context.SaveChanges();
                }
                else if (thongBao != null)
                {
                    // Nếu không còn comment nào -> xoá thông báo
                    _context.ThongBaos.Remove(thongBao);
                    _context.SaveChanges();
                    if (user.ID != baiViet.UserId)
                    {
                        var chatHub = HttpContext.RequestServices.GetService<IHubContext<ChatHub>>();
                        if (chatHub != null)
                        {
                            await chatHub.Clients.User(baiViet.UserId.ToString()).SendAsync("CapNhatBadgeThongBao");
                        }
                    }
                }
            }


            // Load thông tin user của bình luận vừa tạo
            var binhluanMoi = _context.Binhluans
                .Include(bl => bl.User)
                .FirstOrDefault(bl => bl.Id == binhluan.Id);

            string avatarComment = string.IsNullOrEmpty(binhluanMoi.User.Avatar)
                ? "/images/default.png"
                : binhluanMoi.User.Avatar;

            string html = $@"
        <div class='comment-item' id='binhluan-{binhluanMoi.Id}'>
            <img src='{avatarComment}' class='comment-avatar' />
            <div class='comment-content'>
                <div class='comment-header'>
                    <span class='comment-user'>{binhluanMoi.User.Ten}</span>
                    <div class='comment-meta'>
                        <small>{binhluanMoi.Ngaydang.ToString("HH:mm")}</small>
                        <button class='btn-delete-comment' onclick='xoaBinhLuan({binhluanMoi.Id})'>
                            <span class='material-icons-outlined'>delete</span>
                        </button>
                    </div>
                </div>
                <p class='comment-text'>{binhluanMoi.Noidung}</p>
            </div>
        </div>";
            // ===== GỬI SIGNALR CẬP NHẬT BADGE =====
            if (user.ID != baiViet.UserId)
            {
                var chatHub = HttpContext.RequestServices.GetService<IHubContext<ChatHub>>();
                if (chatHub != null)
                {
                    await chatHub.Clients.User(baiViet.UserId.ToString()).SendAsync("CapNhatBadgeThongBao");
                }
            }
            return Json(new
            {
                success = true,
                html = html,
                soLuong = _context.Binhluans.Count(bl => bl.BaivietId == baivietId)
            });
        }


        [HttpPost]
        public async Task<IActionResult> ThichBaiViet(int baiVietId)
        {
            var userSession = HttpContext.Session.GetString("user");
            var user = _context.Users.FirstOrDefault(x => x.EmailorSDT == userSession);

            if (user == null)
            {
                return Json(new { soLuong = 0, daThich = false });
            }

            int nguoiDungId = user.ID;

            // Tìm bài viết để lấy chủ bài
            var baiViet = _context.Baiviets.FirstOrDefault(b => b.Id == baiVietId);
            if (baiViet == null)
            {
                return Json(new { soLuong = 0, daThich = false });
            }

            var thich = _context.Thichs
                .FirstOrDefault(t => t.UserId == nguoiDungId && t.BaivietId == baiVietId);

            bool daThichMoi;

            if (thich != null) // Đã like -> bỏ like
            {
                _context.Thichs.Remove(thich);
                daThichMoi = false;
            }
            else // Chưa like -> thêm like
            {
                _context.Thichs.Add(new Thich
                {
                    UserId = nguoiDungId,
                    BaivietId = baiVietId
                });
                daThichMoi = true;
            }

            // Lưu thay đổi like trước
            _context.SaveChanges();

            // ===== CẬP NHẬT THÔNG BÁO (DÙNG SỐ LƯỢNG THỰC TẾ) =====
            if (nguoiDungId != baiViet.UserId) // Không tự like mình
            {
                // Đếm tổng số like thực tế của bài viết
                var tongSoLike = _context.Thichs.Count(t => t.BaivietId == baiVietId);

                // Tìm thông báo like cho bài viết này
                var thongBao = _context.ThongBaos
                    .FirstOrDefault(t => t.UserId == baiViet.UserId
                                      && t.BaiVietId == baiVietId
                                      && t.Loai == "like");

                if (tongSoLike > 0)
                {
                    if (thongBao != null)
                    {
                        // Cập nhật số lượng = tổng like thực tế
                        thongBao.SoLuong = tongSoLike;
                        thongBao.DaDoc = false;
                        thongBao.ThoiGianCapNhat = DateTime.Now;
                    }
                    else
                    {
                        // Tạo mới với số lượng = tổng like thực tế
                        _context.ThongBaos.Add(new ThongBao
                        {
                            UserId = baiViet.UserId,
                            BaiVietId = baiVietId,
                            Loai = "like",
                            SoLuong = tongSoLike,
                            DaDoc = false,
                            ThoiGianTao = DateTime.Now,
                            ThoiGianCapNhat = DateTime.Now
                        });
                    }
                }
                else
                {
                    // Nếu không còn like nào -> xoá thông báo (nếu có)
                    if (thongBao != null)
                    {
                        _context.ThongBaos.Remove(thongBao);
                    }
                }

                _context.SaveChanges();

            }

            // Đếm lại tổng số like để trả về frontend
            var soLuong = _context.Thichs.Count(t => t.BaivietId == baiVietId);

            // ===== THÊM ĐOẠN NÀY: GỬI SIGNALR CẬP NHẬT BADGE =====
            // Chỉ gửi khi người like không phải chủ bài VÀ đang thực hiện LIKE (không phải unlike)
            if (nguoiDungId != baiViet.UserId && daThichMoi)
            {
                var chatHub = HttpContext.RequestServices.GetService<IHubContext<ChatHub>>();
                if (chatHub != null)
                {
                    await chatHub.Clients.User(baiViet.UserId.ToString()).SendAsync("CapNhatBadgeThongBao");
                    // Thêm dòng debug:
                    Console.WriteLine($"Đã gửi SignalR CapNhatBadgeThongBao đến userId: {baiViet.UserId}");
                }
                else
                {
                    Console.WriteLine("LỖI: chatHub is null!");
                }
            }
            // ===== KẾT THÚC THÊM =====

            return Json(new { soLuong, daThich = daThichMoi });
        }



        // chi tiết bài viết
        public async Task<IActionResult> ChiTiet(int id)
        {
            // Lấy user hiện tại từ session
            string? userEmail = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("DangNhap", "User");
            }

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailorSDT == userEmail);

            if (currentUser == null)
            {
                return RedirectToAction("DangNhap", "User");
            }

            // Lấy bài viết từ database kèm thông tin người đăng và like
            var baiviet = await _context.Baiviets
                .Include(b => b.User)
                .Include(b => b.Thichs)
                .Include(b => b.Binhluans)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            // Nếu không tìm thấy bài viết -> báo lỗi 404
            if (baiviet == null)
            {
                return NotFound();
            }

            // ===== KIỂM TRA QUYỀN: CHỈ CHỦ BÀI MỚI ĐƯỢC XEM =====
            if (baiviet.UserId != currentUser.ID)
            {
                // Nếu không phải chủ bài, chuyển hướng về trang chủ
                TempData["ThongBao"] = "Bạn không có quyền xem bài viết này";
                return RedirectToAction("Index", "Baiviet");
            }

            // Lấy danh sách bài viết đã like của user hiện tại
            var thichs = await _context.Thichs
                .Where(t => t.UserId == currentUser.ID)
                .Select(t => t.BaivietId)
                .ToListAsync();

            ViewBag.Thichs = thichs;
            ViewBag.CurrentUserId = currentUser.ID;

            return View(baiviet);
        }

        [HttpGet]
        public async Task<IActionResult> TaiThemBaiViet(int page = 1, int pageSize = 5)
        {
            var baiviets = _context.Baiviets
                .Include(b => b.User)
                .Include(b => b.Binhluans)
                    .ThenInclude(bl => bl.User)
                .Include(b => b.Thichs)
                .OrderByDescending(b => b.Ngaydang)
                .Skip((page - 1) * pageSize)
                .Take(pageSize + 1) // Lấy thêm 1 để kiểm tra còn không
                .ToList();

            bool hasMore = baiviets.Count > pageSize;
            var postsToReturn = baiviets.Take(pageSize).ToList();

            // Lấy danh sách bài viết đã like của user hiện tại
            var userSession = HttpContext.Session.GetString("user");
            List<int> thichs = new List<int>();

            if (!string.IsNullOrEmpty(userSession))
            {
                var user = _context.Users.FirstOrDefault(u => u.EmailorSDT == userSession);
                if (user != null)
                {
                    thichs = _context.Thichs
                        .Where(t => t.UserId == user.ID)
                        .Select(t => t.BaivietId)
                        .ToList();
                }
            }

            ViewBag.Thichs = thichs;
            int? currentUserId = null;

            if (!string.IsNullOrEmpty(userSession))
            {
                var user = _context.Users.FirstOrDefault(u => u.EmailorSDT == userSession);
                if (user != null)
                    currentUserId = user.ID;
            }

            ViewBag.CurrentUserId = currentUserId;

            string html = "";
            foreach (var bv in postsToReturn)
            {
                html += await RenderPartialViewToStringAsync("Baidang", bv);
            }

            return Json(new
            {
                html = html,
                hasMore = hasMore,
                totalPosts = _context.Baiviets.Count()
            });
        }

        private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                var viewResult = viewEngine.FindView(ControllerContext, viewName, false);

                if (!viewResult.Success)
                {
                    throw new Exception($"Không tìm thấy partial view: {viewName}");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext); // await được phép dùng
                return sw.ToString();
            }
        }
        // === XOÁ BÀI VIẾT ===
        [HttpPost]
        public async Task<IActionResult> XoaBaiViet(int id)
        {
            try
            {
                // Lấy user hiện tại
                var userSession = HttpContext.Session.GetString("user");
                if (string.IsNullOrEmpty(userSession))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);

                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                // Tìm bài viết
                var baiViet = await _context.Baiviets
                    .Include(b => b.Thichs)
                    .Include(b => b.Binhluans)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (baiViet == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài viết" });
                }

                // Kiểm tra quyền: chỉ chủ bài viết mới được xoá
                if (baiViet.UserId != currentUser.ID)
                {
                    return Json(new { success = false, message = "Bạn không có quyền xoá bài viết này" });
                }

                // Xoá ảnh nếu có
                if (!string.IsNullOrEmpty(baiViet.Anh))
                {
                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/baiviet", baiViet.Anh);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Xoá bài viết (EF sẽ tự xoá các Thich và BinhLuan liên quan do Cascade)
                _context.Baiviets.Remove(baiViet);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xoá bài viết" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        public IActionResult TrangCaNhan(int id)
        {
            var user = _context.Users
               .Include(u => u.Baiviets)
               .FirstOrDefault(u => u.ID == id);

            if (user == null)
                return NotFound();

            return View(user);
        }
        [HttpPost]
        public IActionResult KetBan(int nguoiNhanId)
        {
            var userSession = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userSession)) return Json(new { success = false, msg = "Chưa đăng nhập" });

            var userGui = _context.Users.FirstOrDefault(u => u.EmailorSDT == userSession);

            // Kiểm tra xem đã gửi lời mời chưa để tránh trùng lặp
            var daTonTai = _context.KetBans.Any(kb =>
                kb.NguoiGuiId == userGui.ID && kb.NguoiNhanId == nguoiNhanId);

            if (daTonTai) return Json(new { success = false, msg = "Đã gửi lời mời rồi!" });

            var moi = new KetBan
            {
                NguoiGuiId = userGui.ID,
                NguoiNhanId = nguoiNhanId,
                TrangThai = 0, // Trạng thái chờ
                NgayGui = DateTime.Now
            };

            _context.KetBans.Add(moi);
            _context.SaveChanges();

            return Json(new { success = true, msg = "Đã gửi lời mời thành công" });
        }
        // ===== XÓA BÌNH LUẬN =====
        [HttpPost]
        public async Task<IActionResult> XoaBinhLuan(int id)
        {
            try
            {
                var userSession = HttpContext.Session.GetString("user");
                if (string.IsNullOrEmpty(userSession))
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });

                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.EmailorSDT == userSession);
                if (currentUser == null)
                    return Json(new { success = false, message = "Không tìm thấy user" });

                // Tìm bình luận kèm bài viết
                var binhluan = await _context.Binhluans
                    .Include(b => b.Baiviet)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (binhluan == null)
                    return Json(new { success = false, message = "Không tìm thấy bình luận" });

                // Kiểm tra quyền xóa
                bool coQuyenXoa = (binhluan.UserId == currentUser.ID)
                               || (binhluan.Baiviet != null && binhluan.Baiviet.UserId == currentUser.ID);

                if (!coQuyenXoa)
                    return Json(new { success = false, message = "Bạn không có quyền xóa bình luận này" });

                int baivietId = binhluan.BaivietId;

                // Xóa bình luận
                _context.Binhluans.Remove(binhluan);

                // Cập nhật thông báo
                var baiViet = await _context.Baiviets.FindAsync(baivietId);
                if (baiViet != null)
                {
                    var tongSoComment = await _context.Binhluans
                        .CountAsync(bl => bl.BaivietId == baivietId && bl.UserId != baiViet.UserId);

                    var thongBao = await _context.ThongBaos
                        .FirstOrDefaultAsync(t => t.UserId == baiViet.UserId
                                               && t.BaiVietId == baivietId
                                               && t.Loai == "comment");

                    if (tongSoComment > 0)
                    {
                        if (thongBao != null)
                        {
                            thongBao.SoLuong = tongSoComment;
                            thongBao.ThoiGianCapNhat = DateTime.Now;
                        }
                        else
                        {
                            _context.ThongBaos.Add(new ThongBao
                            {
                                UserId = baiViet.UserId,
                                BaiVietId = baivietId,
                                Loai = "comment",
                                SoLuong = tongSoComment,
                                DaDoc = false,
                                ThoiGianTao = DateTime.Now,
                                ThoiGianCapNhat = DateTime.Now
                            });
                        }
                    }
                    else
                    {
                        if (thongBao != null)
                            _context.ThongBaos.Remove(thongBao);
                    }
                }

                // 👉 CHỈ GỌI SaveChangesAsync MỘT LẦN DUY NHẤT
                await _context.SaveChangesAsync();

                int soLuongConLai = await _context.Binhluans.CountAsync(b => b.BaivietId == baivietId);
                return Json(new { success = true, message = "Đã xóa bình luận", soLuong = soLuongConLai });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> LayBinhLuan(int id)
        {
            var binhluans = await _context.Binhluans
                .Include(b => b.User)
                .Where(b => b.BaivietId == id)
                .OrderByDescending(b => b.Ngaydang)
                .ToListAsync();

            // Lấy currentUserId để kiểm tra quyền xóa
            var userSession = HttpContext.Session.GetString("user");
            int? currentUserId = null;
            if (!string.IsNullOrEmpty(userSession))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailorSDT == userSession);
                currentUserId = user?.ID;
            }

            var baiviet = await _context.Baiviets.FindAsync(id);
            ViewBag.BaivietUserId = baiviet?.UserId;
            ViewBag.CurrentUserId = currentUserId;

            return PartialView("_BinhLuanList", binhluans);
        }
    }
}
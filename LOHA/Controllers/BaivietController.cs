using LOHA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

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
                .Where(b => friendIds.Contains(b.UserId)) // <-- CHỈ HIỆN BÀI CỦA BẠN BÈ (TRANGTHAI = 1)
                .OrderByDescending(b => b.Ngaydang)
                .ToList();

            // ===== TRUYỀN DỮ LIỆU SANG VIEW =====
            ViewBag.CurrentUserId = currentUserId;

            return View(baiviets);
        }
        public IActionResult Taobaiviet()
        {
            return View();
        }
        [HttpPost]
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


        // thêm bình luận
        [HttpPost]
        public IActionResult ThemBinhLuan(int baivietId, string noidung, string anchor)
        {
            // Lấy email/sdt từ session
            var userSession = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(userSession))
            {
                // Nếu chưa đăng nhập, trả về lỗi dạng JSON
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            // Tìm user trong DB để lấy ID
            var user = _context.Users.FirstOrDefault(u => u.EmailorSDT == userSession);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy user" });
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

            // Load thông tin user của bình luận vừa tạo (để trả về view)
            var binhluanMoi = _context.Binhluans
                .Include(bl => bl.User)  // Lấy luôn thông tin user
                .FirstOrDefault(bl => bl.Id == binhluan.Id);

            // Tạo HTML cho bình luận mới (sẽ được JavaScript chèn vào trang)
            string html = $@"
        <div class='d-flex mb-3' id='binhluan-{binhluanMoi.Id}'>
            <img src='/images/default.png' class='rounded-circle me-2' style='width:32px;height:32px;object-fit:cover' />
            <div class='bg-light p-2 rounded flex-grow-1'>
                <div class='d-flex justify-content-between'>
                    <b>{binhluanMoi.User.Ten}</b>
                    <small class='text-muted'>{binhluanMoi.Ngaydang.ToString("HH:mm dd/MM")}</small>
                </div>
                <p class='mb-0 mt-1'>{binhluanMoi.Noidung}</p>
            </div>
        </div>";

            // Trả về JSON chứa kết quả
            return Json(new
            {
                success = true,
                html = html,
                soLuong = _context.Binhluans.Count(bl => bl.BaivietId == baivietId)
            });
        }


        // chưa like thì tăng lên 1 đã like thì bỏ like
        [HttpPost]
        public IActionResult ThichBaiViet(int baiVietId)
        {
            // Lấy thông tin người dùng hiện tại từ Session 
            // Session "user" đang lưu email/sđt 
            var userSession = HttpContext.Session.GetString("user");

            // Tìm user trong database dựa vào email/sđt
            var user = _context.Users.FirstOrDefault(x => x.EmailorSDT == userSession);

            // Nếu không tìm thấy user (chưa đăng nhập hoặc session hết hạn)
            if (user == null)
            {
                // Trả về JSON báo lỗi, số like = 0 và trạng thái chưa thích
                return Json(new { soLuong = 0, daThich = false });
            }

            int nguoiDungId = user.ID; // Lấy ID của user để xử lý

            // Kiểm tra xem user đã like bài viết này chưa 
            // Tìm trong bảng Thichs bản ghi có UserId = user hiện tại và BaivietId = bài viết đang thao tác
            var thich = _context.Thichs
                .FirstOrDefault(t => t.UserId == nguoiDungId && t.BaivietId == baiVietId);

            // Biến lưu trạng thái LIKE sau khi xử lý (true = đã thích, false = chưa thích)
            bool daThichMoi;

            //Thực hiện toggle (nếu có thì xóa, không có thì thêm) ---
            if (thich != null) // Đã like trước đó
            {
                // bỏ like
                _context.Thichs.Remove(thich);
                daThichMoi = false; // Sau khi xóa, trạng thái là chưa thích
            }
            else // Chưa like
            {
                // Thêm bản ghi like mới
                _context.Thichs.Add(new Thich
                {
                    UserId = nguoiDungId,
                    BaivietId = baiVietId
                });
                daThichMoi = true; // Sau khi thêm, trạng thái là đã thích
            }

            // Lưu thay đổi vào database
            _context.SaveChanges();

            //  Đếm lại tổng số like của bài viết (để trả về frontend) ---
            var soLuong = _context.Thichs.Count(t => t.BaivietId == baiVietId);

            //Trả kết quả về cho frontend dưới dạng JSON 
            // Trả về cả số lượng like mới VÀ trạng thái like hiện tại của user
            return Json(new { soLuong, daThich = daThichMoi });
        }



        // chi tiết bài viết
        public async Task<IActionResult> ChiTiet(int id)
        {
            // Lấy bài viết từ database kèm thông tin người đăng và like
            var baiviet = _context.Baiviets
                .Include(b => b.User)               // lấy tên người đăng
                .Include(b => b.Thichs)              // lấy danh sách like (để đếm)
                .Include(b => b.Binhluans)            // lấy bình luận (sẽ dùng sau)
                .ThenInclude(b => b.User)           // lấy tên người bình luận
                .FirstOrDefault(b => b.Id == id);

            // Nếu không tìm thấy bài viết -> báo lỗi 404
            if (baiviet == null)
            {
                return NotFound();
            }

            // Kiểm tra user hiện tại đã like bài viết này chưa (cho nút like)
            // Lấy email từ session
            string? userEmail = HttpContext.Session.GetString("user");
            if (!string.IsNullOrEmpty(userEmail))
            {
                // Tìm user bằng email để lấy ID
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmailorSDT == userEmail);

                if (currentUser != null)
                {
                    // Kiểm tra user này đã like bài viết chưa
                    ViewBag.DaThich = await _context.Thichs
                        .AnyAsync(t => t.BaivietId == id && t.UserId == currentUser.ID);
                }
                else
                {
                    ViewBag.DaThich = false;
                }
            }
            else
            {
                ViewBag.DaThich = false;
            }

            // Truyền bài viết sang view
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

    }
}

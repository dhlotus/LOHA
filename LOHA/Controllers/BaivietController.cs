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

        public async Task<IActionResult> Index()
        {
            var baiviets = await _context.Baiviets // lấy tất cả bài viết
                .Include(b => b.User) //lấy thông tin user
                .Include(b => b.Binhluans) //ds comment
                    .ThenInclude(bl => bl.User) // lấy thông tin user của từng comment
                .Include(b => b.Thichs)
                .OrderByDescending(b => b.Ngaydang) // bài viết mới lên đầu
                .ToListAsync();//query và trả về list bài viết

            // Lấy user hiện tại để kiểm tra quyền xoá
            var userSession = HttpContext.Session.GetString("user");
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);
            ViewBag.CurrentUserId = currentUser?.ID ?? 0;

            return View(baiviets);
        }
        public IActionResult Taobaiviet()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Taobaiviet(Baiviet bv, IFormFile AnhFile) // tự động mapping dữ liệu từ form
        {
            var user = HttpContext.Session.GetString("user"); //lưu dữ liệu tạm của người dùng sau khi login

            if (user == null) // chưa đăng nhập chuyển tới trang đăng nhập
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
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(AnhFile.FileName); // tạo tên file ngẫu nhiên
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/baiviet", filename); // đường dẫn lưu file

                using (var stream = new FileStream(path, FileMode.Create)) // lưu file trên ổ cứn
                {
                    AnhFile.CopyTo(stream); // coppy dữ liệu upload vào file
                }
                bv.Anh = filename; // lưu tên ảnh vào db
            }
            bv.Ngaydang = DateTime.Now; // lưu thời gian đăng 

            _context.Baiviets.Add(bv); // thêm bản ghi 
            _context.SaveChanges(); //ghi vào db

            return RedirectToAction("Trangcanhan", "User");
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
        // chi tiết bài viết
        public IActionResult ChiTiet(int id)
        {
            // Lấy bài viết từ database kèm thông tin người đăng và like
            var baiviet = _context.Baiviets
                .Include(b => b.User)
                .Include(b => b.Thichs)
                .Include(b => b.Binhluans)
                    .ThenInclude(b => b.User)
                .FirstOrDefault(b => b.Id == id);

            if (baiviet == null)
            {
                return NotFound();
            }

            // Lấy email từ session, sau đó tìm user để lấy ID
            string? userEmail = HttpContext.Session.GetString("user");
            if (!string.IsNullOrEmpty(userEmail))
            {
                var user = _context.Users.FirstOrDefault(u => u.EmailorSDT == userEmail);
                if (user != null)
                {
                    ViewBag.DaThich = _context.Thichs
                        .Any(t => t.BaivietId == id && t.UserId == user.ID);
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

            return View(baiviet);
        }

        [HttpGet]
        public async Task<IActionResult> TaiThemBaiViet(int page = 1, int pageSize = 5)
        {
            var baiviets = await _context.Baiviets
                .Include(b => b.User)
                .Include(b => b.Binhluans)
                    .ThenInclude(bl => bl.User)
                .Include(b => b.Thichs)
                .OrderByDescending(b => b.Ngaydang)
                .Skip((page - 1) * pageSize)
                .Take(pageSize + 1)
                .ToListAsync();

            bool hasMore = baiviets.Count > pageSize;
            var postsToReturn = baiviets.Take(pageSize).ToList();

            // Lấy danh sách bài viết đã like của user hiện tại
            var userSession = HttpContext.Session.GetString("user");
            List<int> thichs = new List<int>();
            int currentUserId = 0;

            if (!string.IsNullOrEmpty(userSession))
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmailorSDT == userSession);
                if (user != null)
                {
                    currentUserId = user.ID;
                    thichs = await _context.Thichs
                        .Where(t => t.UserId == user.ID)
                        .Select(t => t.BaivietId)
                        .ToListAsync();
                }
            }

            ViewBag.Thichs = thichs;
            ViewBag.CurrentUserId = currentUserId; // 👈 THÊM DÒNG NÀY

            string html = "";
            foreach (var bv in postsToReturn)
            {
                html += await RenderPartialViewToStringAsync("Baidang", bv);
            }

            return Json(new
            {
                html = html,
                hasMore = hasMore,
                totalPosts = await _context.Baiviets.CountAsync()
            });
        }

        private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            // Truyền ViewBag.CurrentUserId sang Partial View
            ViewBag.CurrentUserId = ViewBag.CurrentUserId;

            using (var sw = new StringWriter())
            {
                var viewEngine = HttpContext.RequestServices
                    .GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
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

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
        // === XOÁ BÀI VIẾT ===
        [HttpPost]
        public async Task<IActionResult> XoaBaiViet(int id)
        {
            try
            {
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

                var baiViet = await _context.Baiviets
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

                _context.Baiviets.Remove(baiViet);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xoá bài viết" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

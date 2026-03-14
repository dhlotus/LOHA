using Microsoft.AspNetCore.Mvc;
using makefb.Models;
using Microsoft.EntityFrameworkCore;

namespace makefb.Controllers
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
            var baiviets = _context.Baiviets // lấy tất cả bài viết
                .Include(b => b.User) //lấy thông tin user
                .Include(b => b.Binhluans) //ds comment
                .ThenInclude(bl => bl.User) // lấy thông tin user của từng comment
                .OrderByDescending(b => b.Ngaydang) // bài viết mới lên đầu
                .ToList(); //query và trả về list bài viết

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

            if(user == null) // chưa đăng nhập chuyển tới trang đăng nhập
            {
                return RedirectToAction("DangNhap", "User");
            }
            bv.UserId = int.Parse(user); // gán userid cho bài viết
            if(AnhFile != null)
            {
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(AnhFile.FileName); // tạo tên file ngẫu nhiên
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/baiviet", filename); // đường dẫn lưu file

                using(var stream = new FileStream(path, FileMode.Create)) // lưu file trên ổ cứn
                {
                    AnhFile.CopyTo(stream); // coppy dữ liệu upload vào file
                }
                bv.Anh = filename; // lưu tên ảnh vào db
            }
            bv.Ngaydang = DateTime.Now; // lưu thời gian đăng 

            _context.Baiviets.Add(bv); // thêm bản ghi 
            _context.SaveChanges(); //ghi vào db

            return RedirectToAction("Index");
        }
        [HttpPost]
        public JsonResult Thich(int id) // id của bài viết
        {
            var baiviet = _context.Baiviets.Find(id); // tìm bài viết trong db
            if(baiviet != null)
            {
                baiviet.Luotthich += 1; // tăng lượt thích;
                _context.SaveChanges(); // lưu thay đổi bào db

                return Json(new { luothich = baiviet.Luotthich }); // server gửi lai jsoos like mới
            }

            return Json(new { luotthich = 0});
        }

        // thêm bình luận
        [HttpPost]
        public IActionResult ThemBinhLuan(int baivietId, string noidung, string anchor) //anchor id của bv
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("DangNhap", "User");
            }

            var binhluan = new Binhluan
            {
                BaivietId = baivietId,
                UserId = userId.Value,
                Noidung = noidung,
                Ngaydang = DateTime.Now
            };

            _context.Binhluans.Add(binhluan);
            _context.SaveChanges();

            return Redirect("/Baiviet/Index#" + anchor); // cuộn tới bv đó
        }
        // trang chi tiết bài viết
        public IActionResult ChiTiet(int id)
        {
            var baiviet = _context.Baiviets
                .Include(b => b.User) // lấy người đăng bài
                .Include(b => b.Binhluans) // lấy danh sách bình luận
                .ThenInclude(bl => bl.User) // lấy thông tin người bình luận
                .FirstOrDefault(b => b.Id == id); // tìm bài viết theo id

            if (baiviet == null) // nếu không tìm thấy bài viết, trả về trang lỗi
            {
                return NotFound();
            }

            return View(baiviet);
        }
    }
}

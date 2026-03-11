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
            var danhsach = _context.Baiviets // lấy tất cả bài viết
                .Include(x => x.User) // lấy thông tin user của bài viết) 
                .OrderByDescending(x => x.Ngaydang) // sắp xếp mới nhất trước
                .ToList(); // chuyển thành list

            return View(danhsach);
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
        public IActionResult ThemBinhLuan(int baivietId, string noidung) // bình luận thuộc bài viết nào và nội dung bình luận là gì
        {
            var user = HttpContext.Session.GetString("user"); // lấy id user đang đăng nhập

            if (user == null)
            {
                return RedirectToAction("DangNhap", "User");
            }

            int userId = int.Parse(user);

            Binhluan bl = new Binhluan(); // tạo oj bình luận, sau đó gán dữ liệu
            bl.Noidung = noidung;
            bl.Ngaydang = DateTime.Now;
            bl.UserId = userId;
            bl.BaivietId = baivietId;

            // lưu vào db
            _context.Binhluans.Add(bl);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}

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
                .Include(x => x.User)
                .OrderByDescending(x => x.Ngaydang) // sắp xếp mới nhất trước
                .ToList(); // chuyển thành list

            return View();
        }
        public IActionResult Taobaiviet()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Taobaiviet(Baiviet bv, IFormFile AnhFile) // ảnh user up
        {
            var user = HttpContext.Session.GetString("user");

            if(user == null)
            {
                return RedirectToAction("DangNhap", "User");
            }

            if(AnhFile != null)
            {
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(AnhFile.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/baiviet", filename);

                using(var stream = new FileStream(path, FileMode.Create))
                {
                    AnhFile.CopyTo(stream);
                }
                bv.Anh = filename;
            }
            bv.Ngaydang = DateTime.Now;

            _context.Baiviets.Add(bv);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}

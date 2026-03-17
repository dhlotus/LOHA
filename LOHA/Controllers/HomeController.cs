using LOHA.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LOHA.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("UserEmail"); // lấy email đã lưu vào sesstion
            if(email == null) // nếu không có email trong sesstion, nghĩa là chưa đăng nhập
            {
                return RedirectToAction("DangNhap", "User"); // trả về trang đăng nhập
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

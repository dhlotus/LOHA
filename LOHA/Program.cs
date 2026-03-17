using LOHA.Models; // sừ dụng các class trong thư viện models
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); // khởi tạo ứng dụng ASP

builder.Services.AddControllersWithViews(); //bật mô hình MVC cho  pj
builder.Services.AddSession(); // bật chức năng sesstion

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // kết nối pj với db sql server

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); // chuyển http sang https
app.UseRouting(); // bật chức năng định tuyến
app.UseSession(); // cho phép dùng sesstion trong pj

app.UseAuthorization(); // bật chức năng kiểm tra quyền truy cập

app.MapStaticAssets(); // sử dụng các file tĩnh trong pj

app.MapControllerRoute( // định nghĩa đường dẫn URL cho controller
    name: "default",// đặt tên cho routr
    pattern: "{controller=User}/{action=Trangcanhan}/{id?}") // url của website, mặc định vào usercontroller, mặc định chạy action đăng ký, id tuỳ chọn
    .WithStaticAssets(); // cho phép route sử dụng file tĩnh css jss


app.Run(); // khởi chạy website

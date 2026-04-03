using LOHA.Hubs;
using LOHA.Models; // sử dụng các class trong thư viện models
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); // khởi tạo ứng dụng ASP

// ===== THÊM CÁC DỊCH VỤ (SERVICES) =====
builder.Services.AddControllersWithViews(); // bật mô hình MVC cho project
builder.Services.AddSession(); // bật chức năng session (lưu trạng thái đăng nhập)

// Kết nối database SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR(); // bật chức năng realtime với SignalR (chat)

var app = builder.Build(); // xây dựng ứng dụng

// ===== TỰ ĐỘNG CẬP NHẬT DATABASE KHI CHẠY =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // chạy các migration chưa được áp dụng
}

// ===== CẤU HÌNH PIPELINE XỬ LÝ REQUEST =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // xử lý lỗi khi chạy production
    app.UseHsts(); // bảo mật HTTPS
}

app.UseHttpsRedirection(); // chuyển http sang https
app.UseRouting(); // bật chức năng định tuyến (routing)
app.UseSession(); // cho phép dùng session trong project
app.UseAuthorization(); // bật chức năng kiểm tra quyền truy cập

app.MapHub<ChatHub>("/chathub"); // định nghĩa đường dẫn cho hub realtime (chat)
app.MapStaticAssets(); // sử dụng các file tĩnh trong project (css, js, ảnh...)

// Định nghĩa route mặc định
app.MapControllerRoute(
    name: "default", // đặt tên cho route
    pattern: "{controller=User}/{action=Trangcanhan}/{id?}") // url mặc định: vào trang cá nhân
    .WithStaticAssets(); // cho phép route sử dụng file tĩnh

app.Run(); // khởi chạy website
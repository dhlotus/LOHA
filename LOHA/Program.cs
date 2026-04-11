using LOHA.Data;
using LOHA.Hubs;
using LOHA.Models;
using LOHA.Services; 
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// ===== THÊM CÁC DỊCH VỤ (SERVICES) =====
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

// Kết nối database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR();

// Cấu hình Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();

var app = builder.Build(); // xây dựng ứng dụng

// ===== TỰ ĐỘNG CẬP NHẬT DATABASE KHI CHẠY =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // chạy các migration chưa được áp dụng
}
// Gọi Seeder để tạo dữ liệu mẫu
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbInitializer.Initialize(dbContext);
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
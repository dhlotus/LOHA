using Microsoft.AspNetCore.SignalR;
using LOHA.Models;
using Microsoft.EntityFrameworkCore;

namespace LOHA.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        // Hàm gửi tin nhắn realtime
        public async Task GuiTinNhan(int nguoiGuiId, int nguoiNhanId, string noiDung, DateTime thoiGian)
        {
            // Tạo ID phòng chat giữa 2 người
            string roomId = nguoiGuiId < nguoiNhanId
                ? $"chat-{nguoiGuiId}-{nguoiNhanId}"
                : $"chat-{nguoiNhanId}-{nguoiGuiId}";

            // Tạo HTML cho tin nhắn
            string html = $@"
                <div class='d-flex justify-content-end mb-3'>
                    <div class='bg-primary text-white p-3 rounded-3' style='max-width: 70%;'>
                        <p class='mb-1'>{noiDung}</p>
                        <div class='text-end'>
                            <small class='text-white-50'>{thoiGian.ToString("HH:mm")}</small>
                        </div>
                    </div>
                </div>";

            // Gửi tin nhắn đến phòng chat
            await Clients.Group(roomId).SendAsync("NhanTinMoi", html, nguoiGuiId, thoiGian);
        }

        // Khi client kết nối
        public override async Task OnConnectedAsync()
        {
            // Lấy userId từ query string (sẽ truyền từ client)
            var httpContext = Context.GetHttpContext();
            var userId = httpContext.Request.Query["userId"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                // Join vào tất cả các phòng chat liên quan đến user này
                var cacPhongChat = await _context.TinNhans
                    .Where(t => t.NguoiGuiID.ToString() == userId || t.NguoiNhanID.ToString() == userId)
                    .Select(t => t.NguoiGuiID < t.NguoiNhanID
                        ? $"chat-{t.NguoiGuiID}-{t.NguoiNhanID}"
                        : $"chat-{t.NguoiNhanID}-{t.NguoiGuiID}")
                    .Distinct()
                    .ToListAsync();

                foreach (var phong in cacPhongChat)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, phong);
                }
            }

            await base.OnConnectedAsync();
        }

        // Khi client ngắt kết nối
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Xử lý khi ngắt kết nối nếu cần
            await base.OnDisconnectedAsync(exception);
        }
        // Method để client join vào phòng chat
        public async Task JoinPhongChat(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }
        // Gửi sự kiện cập nhật danh sách chat cho người nhận
        public async Task CapNhatDanhSachChat(int nguoiNhanId)
        {
            await Clients.User(nguoiNhanId.ToString()).SendAsync("CapNhatDanhSachChat");
        }
        // Gửi sự kiện cập nhật badge thông báo cho user
        public async Task CapNhatBadgeThongBao(int userId)
        {
            await Clients.User(userId.ToString()).SendAsync("CapNhatBadgeThongBao");
        }
        // Gửi tín hiệu force logout đến user bị khóa
        public async Task ForceLogout(int userId, string message)
        {
            await Clients.User(userId.ToString()).SendAsync("ForceLogout", message);
        }
    }
}
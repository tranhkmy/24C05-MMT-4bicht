

**Có 2 cách để chạy: Chạy trên 1 máy (Localhost) và Chạy trên 2 máy (Mạng LAN).**
________________________________________
**CÁCH 1: Test trên cùng 1 máy (Localhost)**

**1. Chuẩn bị Code (Máy B – bị điều khiển):**

• Mở file server.cs.

• Tìm hàm StartServer, đảm bảo dòng IP là: 
IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5656);

•	Nhấn Build.

**2. Chạy Web Admin (Backend):**

•	Mở folder WebcamBackend lên, nhấn Play.

•	Trình duyệt hiện lên. Chờ dòng trạng thái: "Status: Connected to Hub...".

**3. Chạy Máy B:**

•	Mở file server.cs trong thư mục server lên nhấn Play.

•	Lúc đó: Cửa sổ Console của Web sẽ hiện dòng [TCP Server] May server da ket noi....

**4. Thao tác test webcam:**

•	Trên Web, bấm nút "BẬT WEBCAM".

•	Đèn Camera sáng và ảnh hiện lên là Thành công.
________________________________________
**CÁCH 2: Test trên 2 máy khác nhau (Mạng LAN)
--> CÁCH NÀY CHƯA CHẠY THỬ NÊN K BIẾT**

**1. Lấy IP Máy A (Admin):**

•	Trên Máy A, mở CMD gõ ipconfig.

•	Ghi lại địa chỉ IPv4 (Ví dụ: 192.168.1.15).

**2. Cấu hình Máy A:**

•	Tắt Tường lửa (Firewall) hoặc cho phép cổng 5656 và 5000/5001.

•	Mở file Properties/launchSettings.json trong Project Web, sửa applicationUrl thành: "http://0.0.0.0:5000".

•	Chạy Web Server lên.

**3. Cấu hình Code Máy B:**

•	Mở file server.cs.

•	Sửa IP thành IP của máy A:
IPEndPoint ip = new IPEndPoint(IPAddress.Parse("192.168.1.15"), 5656);

•	Rebuild (F6).

•	Copy toàn bộ thư mục bin/Debug (chứa server.exe và các file AForge.dll) sang Máy B.

**4. Chạy thực chiến:**

•	Máy B: Chạy file server.exe $\rightarrow$ Bấm Start Server.

•	Máy A: Mở trình duyệt, truy cập http://localhost:5000 (hoặc http://192.168.1.15:5000).

•	Thấy báo kết nối thành công $\rightarrow$ Bấm "BẬT WEBCAM".

•	Webcam máy B sẽ bật và truyền hình về máy A.


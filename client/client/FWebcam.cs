//using AForge.Video;
//using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace client
{
    public partial class FWebcam : Form
    {
        public FWebcam()
        {
            InitializeComponent();
        }

        private Thread listenThread;
        private bool isListening = true;



        private void Webcam_Load(object sender, EventArgs e)
        {
            listenThread = new Thread(new ThreadStart(ReceiveImage));
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        // Hàm nhận ảnh
        private void ReceiveImage()
        {
            try
            {
                while (isListening)
                {
                    // 1. Đọc kích thước ảnh
                    //string sizeLine = Program.nr.ReadLine();
                    //if (string.IsNullOrEmpty(sizeLine)) break;
                    string sizeLine = "";
                    byte[] oneByte = new byte[1];

                    while (true)
                    {
                        int r = Program.ns.Read(oneByte, 0, 1);
                        if (r == 0) return; // Mất kết nối

                        char c = (char)oneByte[0];
                        if (c == '\n') break; // Gặp ký tự xuống dòng thì dừng
                        if (c != '\r') sizeLine += c; // Ghép từng ký tự thành số
                    }

                    int size = 0;
                    if (!int.TryParse(sizeLine, out size)) continue;

                    byte[] buffer = new byte[size];
                    int totalRead = 0;

                    // 2. Đọc dữ liệu ảnh 
                    while (totalRead < size)
                    {
                        int read = Program.ns.Read(buffer, totalRead, size - totalRead);
                        if (read == 0) break;
                        totalRead += read;
                    }

                    // 3. Hiển thị ảnh ra màn hình 
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        Image img = Image.FromStream(ms);
                        img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        try
                        {
                            pictureBox1.Invoke((MethodInvoker)delegate
                            {
                                pictureBox1.Image = img;
                            });
                        }
                        catch { break; }
                    }
                }
            }
            catch
            {
                this.Invoke((MethodInvoker)delegate { this.Close(); });
            }
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            StopAndClose();
            this.Close();
        }

        private void Webcam_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopAndClose();
        }

        private void StopAndClose()
        {
            if (isListening)
            {
                isListening = false;
                try
                {
                    Program.nw.WriteLine("QUIT");
                    Program.nw.Flush();
                }
                catch { }
            }
        }
    }
}

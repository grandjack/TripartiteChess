using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for UserInfoWin.xaml
    /// </summary>
    public partial class UserInfoWin : Window
    {
        private bool mRestoreIfMove = false;
        public UserInfoWin()
        {
            InitializeComponent();

            this.tx_nickname.Text = GameState.currentUserName;
            this.tx_password.Password = GameState.currentUserPassword;
            this.tx_passwordRe.Password = GameState.currentUserPassword;
            this.tx_account.Text = GameState.currentUserAccount;
            this.tx_email.Text = GameState.currentUserEmail;
            this.tx_phone.Text = GameState.currentUserPhoneNo;
        }

        private void Close_Window(object sender, RoutedEventArgs e)
        {
            this.Close();
            e.Handled = true;
        }

        private void Min_window(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                this.WindowState = WindowState.Minimized;
                e.Handled = true;
            }
        }

        //*************************
        // *****自定义窗口实现最大化不覆盖任务栏
        // **************************
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr mWindowHandle = (new WindowInteropHelper(this)).Handle;
            HwndSource.FromHwnd(mWindowHandle).AddHook(new HwndSourceHook(WindowProc));
        }
        private static System.IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        private static void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {
            POINT lMousePosition;
            GetCursorPos(out lMousePosition);

            IntPtr lPrimaryScreen = MonitorFromPoint(new POINT(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);
            MONITORINFO lPrimaryScreenInfo = new MONITORINFO();
            if (GetMonitorInfo(lPrimaryScreen, lPrimaryScreenInfo) == false)
            {
                return;
            }

            IntPtr lCurrentScreen = MonitorFromPoint(lMousePosition, MonitorOptions.MONITOR_DEFAULTTONEAREST);

            MINMAXINFO lMmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            if (lPrimaryScreen.Equals(lCurrentScreen) == true)
            {
                lMmi.ptMaxPosition.X = lPrimaryScreenInfo.rcWork.Left;
                lMmi.ptMaxPosition.Y = lPrimaryScreenInfo.rcWork.Top;
                lMmi.ptMaxSize.X = lPrimaryScreenInfo.rcWork.Right - lPrimaryScreenInfo.rcWork.Left;
                lMmi.ptMaxSize.Y = lPrimaryScreenInfo.rcWork.Bottom - lPrimaryScreenInfo.rcWork.Top;
            }
            else
            {
                lMmi.ptMaxPosition.X = lPrimaryScreenInfo.rcMonitor.Left;
                lMmi.ptMaxPosition.Y = lPrimaryScreenInfo.rcMonitor.Top;
                lMmi.ptMaxSize.X = lPrimaryScreenInfo.rcMonitor.Right - lPrimaryScreenInfo.rcMonitor.Left;
                lMmi.ptMaxSize.Y = lPrimaryScreenInfo.rcMonitor.Bottom - lPrimaryScreenInfo.rcMonitor.Top;
            }

            Marshal.StructureToPtr(lMmi, lParam, true);
        }

        private void SwitchWindowState()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    {
                        WindowState = WindowState.Maximized;
                        break;
                    }
                case WindowState.Maximized:
                    {
                        WindowState = WindowState.Normal;
                        break;
                    }
                default:
                    break;
            }
        }


        private void rctHeader_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if ((ResizeMode == ResizeMode.CanResize) || (ResizeMode == ResizeMode.CanResizeWithGrip))
                {
                    SwitchWindowState();
                }

                return;
            }

            else if (WindowState == WindowState.Maximized)
            {
                mRestoreIfMove = true;
                return;
            }

            DragMove();
            e.Handled = true;
        }


        private void rctHeader_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mRestoreIfMove = false;
            e.Handled = true;
        }


        private void rctHeader_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mRestoreIfMove)
            {
                mRestoreIfMove = false;

                double percentHorizontal = e.GetPosition(this).X / ActualWidth;
                double targetHorizontal = RestoreBounds.Width * percentHorizontal;

                double percentVertical = e.GetPosition(this).Y / ActualHeight;
                double targetVertical = RestoreBounds.Height * percentVertical;

                WindowState = WindowState.Normal;

                POINT lMousePosition;
                GetCursorPos(out lMousePosition);

                Left = lMousePosition.X - targetHorizontal;
                Top = lMousePosition.Y - targetVertical;

                DragMove();
                e.Handled = true;
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);


        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

        enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }


        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);


        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        

        private void UserInfoWinClosingHand(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private Image headImage = null;
        private byte[] headImageBytes = null;
        private void UploadHeadImage(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            //dialog.Filter = "PNG File(*.png)|*.png";
            dialog.Filter = "图片文件(*.png,*.jpg,*.gif,*.bmp)|*.png;*.jpg;*.gif;*.bmp";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == true)
            {
                FileStream fs = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                if ((int)fs.Length > 200*1024)//must less than 200KB per picture
                {
                    MessageBox.Show("The Image is too big (max size 200KB)!!!  Current size: " + (fs.Length/1024).ToString() + "KB");
                    return;
                }

                headImageBytes = br.ReadBytes((int)fs.Length);
                string file_name = dialog.FileName;
                br.Close();
                fs.Close();

                DisplayHeadImageToSettingBoard(headImageBytes);
            }
        }

        public void DisplayHeadImageToSettingBoard(byte[] byteArray)
        {
            //set image source
            if (headImage != null)
            {
                settingBoard.Children.Remove(headImage);
            }

            Image headImg = new Image();
            headImg.Source = ByteArrayToBitmapImage(byteArray);
            headImg.HorizontalAlignment = HorizontalAlignment.Left;
            headImg.VerticalAlignment = VerticalAlignment.Top;
            headImg.Margin = new Thickness(32, 22, 0, 0);
            headImg.Width = 101;
            headImg.Height = 80;
            headImg.Stretch = Stretch.Uniform;

            headImage = headImg;
            
            settingBoard.Children.Add(headImage);
        }

        BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            BitmapImage bmp = null;

            try
            {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(byteArray);
                bmp.EndInit();
            }
            catch
            {
                bmp = null;
            }

            return bmp;
        }

        private void textChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine("Text changed!");
        }

        private void PwdTextChanged(object sender, RoutedEventArgs e)
        {
            if (!tx_password.Password.Equals(GameState.currentUserPassword))
            {
                Console.WriteLine("pwd changed!");
                lbpwdRetry.Visibility = System.Windows.Visibility.Visible;
                tx_passwordRe.Visibility = System.Windows.Visibility.Visible;
                tx_passwordRe.Password = "";
            }
        }

        private void EditUserInfoClick(object sender, RoutedEventArgs e)
        {
            tx_password.IsEnabled = true;
            tx_email.IsEnabled = true;
            tx_phone.IsEnabled = true;
            tx_nickname.IsEnabled = true;
            tx_passwordRe.IsEnabled = true;
        }

        private void SaveUserInfoClick(object sender, RoutedEventArgs e)
        {
            tx_password.IsEnabled = false;
            tx_email.IsEnabled = false;
            tx_phone.IsEnabled = false;
            tx_nickname.IsEnabled = false;
            tx_passwordRe.IsEnabled = false;


            lbpwdRetry.Visibility = System.Windows.Visibility.Hidden;
            tx_passwordRe.Visibility = System.Windows.Visibility.Hidden;

            if (tx_password.Password.Equals(tx_passwordRe.Password))
            {
                MD5Hash hash = new MD5Hash(tx_password.Password);
                GameReadyState s = new GameReadyState();
                s.UpdateUserInfos(tx_account.Text, tx_nickname.Text, hash.GetMD5HashCode(), tx_email.Text, tx_phone.Text, headImageBytes);
            }
            else
            {
                MessageBox.Show("Password are NOT equal!  " + tx_password.Password.ToString() + "\n\rRePwd:" + tx_passwordRe.Password.ToString());
            }
        }

        private void SettingBoardLoad(object sender, RoutedEventArgs e)
        {
            if (GameState.currentUserHeadImage != null)
            {
                DisplayHeadImageToSettingBoard(GameState.currentUserHeadImage);
            }
        }
    }
}

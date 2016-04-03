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
using System.Windows.Controls.Primitives;


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
            //this.tx_password.Password = GameState.currentUserPassword;
            //this.tx_passwordRe.Password = GameState.currentUserPassword;

            this.tx_password.Password = GameState.currentUserActualPassword;
            this.tx_passwordRe.Password = GameState.currentUserActualPassword;
            this.tx_account.Text = GameState.currentUserAccount;
            this.tx_email.Text = GameState.currentUserEmail;
            this.tx_phone.Text = GameState.currentUserPhoneNo;
            this.tx_score.Text = GameState.currentUserScore.ToString();
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
            if (text_changed && !saved && (changed_num > 3))
            {
                MessageBoxResult resut = MyMessageBox.Show(this, "您的修改未保存，确定要退出吗", "提示" ,MessageBoxButton.OKCancel);
                if (resut == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    text_changed = false;
                    saved = false;
                }
            }
        }

        //private Image headImage = null;
        private byte[] headImageBytes = null;
        private void UploadHeadImage(object sender, MouseButtonEventArgs e)
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
                    WindowShowTimer box = new WindowShowTimer(this, "警告", "头像图片不能超过200KB");
                    box.Show();
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
            Image headImg = new Image();
            headImg.Source = ByteArrayToBitmapImage(byteArray);
            headImg.HorizontalAlignment = HorizontalAlignment.Stretch;
            headImg.VerticalAlignment = VerticalAlignment.Stretch;
            //headImg.Margin = new Thickness(32, 22, 0, 0);
            headImg.Stretch = Stretch.Uniform;
            
            head_image_board.Background = new ImageBrush(headImg.Source);
            
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

        private bool text_changed = false;
        private int changed_num = 0;
        private bool saved = false;
        private void textChanged(object sender, TextChangedEventArgs e)
        {
            TextBlock tt = sender as TextBlock;
            Console.WriteLine("Text changed!");
            text_changed = true;
            ++changed_num;
        }

        private int paw_changed_count = 0;
        private void PwdTextChanged(object sender, RoutedEventArgs e)
        {
            if (!tx_password.Password.Equals(GameState.currentUserActualPassword))
            {
                Console.WriteLine("pwd changed!");
                lbpwdRetry.Visibility = System.Windows.Visibility.Visible;
                tx_passwordRe.Visibility = System.Windows.Visibility.Visible;
                tx_passwordRe.Password = "";
                ++paw_changed_count;
            }
        }
        
        private void SaveUserInfoClick(object sender, RoutedEventArgs e)
        {
            if (tx_password.Password.Equals(tx_passwordRe.Password))
            {
                lbpwdRetry.Visibility = System.Windows.Visibility.Hidden;
                tx_passwordRe.Visibility = System.Windows.Visibility.Hidden;
                saved = true;

                string pwd_md5 = "";
                if (GameState.logginWin.NeedRecordPwd() && (paw_changed_count < 2))
                {
                    pwd_md5 = GameState.currentUserPassword;
                }
                else
                {
                    MD5Hash hash = new MD5Hash(tx_password.Password);
                    pwd_md5 = hash.GetMD5HashCode();
                }

                GameReadyState s = new GameReadyState();
                s.UpdateUserInfos(tx_account.Text, tx_nickname.Text, pwd_md5, tx_email.Text, tx_phone.Text, headImageBytes);
                this.Close();
            }
            else
            {
                WindowShowTimer box = new WindowShowTimer(this, "警告", "前后两次密码不一致，请重新输入！");
                box.Show();
            }
        }

        private void SettingBoardLoad(object sender, RoutedEventArgs e)
        {
            if (GameState.currentUserHeadImage != null)
            {
                DisplayHeadImageToSettingBoard(GameState.currentUserHeadImage);
            }
            else
            {
                BitmapImage bit = new BitmapImage();
                bit.BeginInit();
                bit.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\MyIcon.png");
                bit.EndInit();

                Image headImg = new Image();
                headImg.Source = bit;
                headImg.HorizontalAlignment = HorizontalAlignment.Stretch;
                headImg.VerticalAlignment = VerticalAlignment.Stretch;
                //headImg.Margin = new Thickness(32, 22, 0, 0);
                headImg.Stretch = Stretch.Uniform;

                head_image_board.Background = new ImageBrush(headImg.Source);
            }
        }
        
        private void EmailGotFocus(object sender, RoutedEventArgs e)
        {
            if (tx_email.IsFocused)
            {
                tx_email.Background = Brushes.White;
                tx_email.SelectionStart = tx_email.Text.Length;
            }
        }

        private void PhoneGotFocus(object sender, RoutedEventArgs e)
        {
            if (tx_phone.IsFocused)
            {
                tx_phone.Background = Brushes.White;
                tx_phone.SelectionStart = tx_phone.Text.Length;
            }
        }

        private void EmailLostFocus(object sender, RoutedEventArgs e)
        {
            if (!tx_email.IsFocused)
            {
                tx_email.Background = Brushes.Transparent;
            }
        }

        private void PhoneLostFocus(object sender, RoutedEventArgs e)
        {
            if (!tx_phone.IsFocused)
            {
                tx_phone.Background = Brushes.Transparent;
            }
        }

        private void NickNameGotFocus(object sender, RoutedEventArgs e)
        {
            if (tx_nickname.IsFocused)
            {
                tx_nickname.Background = Brushes.White;
                tx_nickname.SelectionStart = tx_nickname.Text.Length;
            }
        }

        private void NickNameLostFocus(object sender, RoutedEventArgs e)
        {
            if (!tx_nickname.IsFocused)
            {
                tx_nickname.Background = Brushes.Transparent;
            }
        }

        private void PwdGotFocus(object sender, RoutedEventArgs e)
        {
            if (tx_password.IsFocused)
            {
                tx_password.Background = Brushes.White;
            }
        }

        private void PwdLostFocus(object sender, RoutedEventArgs e)
        {
            if (!tx_password.IsFocused)
            {
                tx_password.Background = Brushes.Transparent;
            }
        }

        private void RePwdLostFocus(object sender, RoutedEventArgs e)
        {
            if (!tx_passwordRe.IsFocused)
            {
                tx_passwordRe.Background = Brushes.Transparent;
                lbpwdRetry.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void RePwdGotFocus(object sender, RoutedEventArgs e)
        {
            if (tx_passwordRe.IsFocused)
            {
                tx_passwordRe.Background = Brushes.White;
                lbpwdRetry.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}

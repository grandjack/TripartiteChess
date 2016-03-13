using System;
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
    /// Interaction logic for Chess.xaml
    /// </summary>
    public partial class Chess : Window
    {
        private bool mRestoreIfMove = false;

        public Chess()
        {
            InitializeComponent();
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
        
        private void Normal_Window(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;
                max_win2.Visibility = Visibility.Collapsed;
                normal_win2.Visibility = Visibility.Visible;
            }
            else if (this.WindowState != WindowState.Normal)
            {
                this.WindowState = WindowState.Normal;
                max_win2.Visibility = Visibility.Visible;
                normal_win2.Visibility = Visibility.Collapsed;
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

        private void HidewebChat(object sender, RoutedEventArgs e)
        {
            if (this.webChat.Visibility != Visibility.Hidden)
            {
                this.webChat.Visibility = Visibility.Hidden;
            }
            if (this.chat_btn.Visibility != Visibility.Visible)
            {
                this.chat_btn.Visibility = Visibility.Visible;
            }
        }

        private void Chat_Click(object sender, RoutedEventArgs e)
        {
            if (this.webChat.Visibility != Visibility.Visible)
            {
                this.webChat.Visibility = Visibility.Visible;
            }

            if (this.chat_btn.Visibility != Visibility.Hidden)
            {
                this.chat_btn.Visibility = Visibility.Hidden;
            }
        }
        //自定义窗口实现最大化不覆盖任务栏--End

        public void ChessBoardLoaded(object sender, RoutedEventArgs e)
        {
            gridChessBoard.Children.Clear();
            //ChessMan chess = new Horse;
            Console.WriteLine("GameState.locate = " + GameState.locate);
            ChessBoard chessBoard = ChessBoard.GetChessBoardObj((Location)GameState.locate, this);
            //ChessBoard chessBoard = ChessBoard.GetChessBoardObj(Location.bottom, this);
            /*for (int row = 0; row < 14; ++row)
            {
                for (int column = 0; column < 19; ++column)
                {
                    Thickness margin = new Thickness(4.5 + column * 50.4, 4 + row * 50.4, 0, 0);
                    this.draw_seat_image(margin);
                }
            }
            */
        }

        private void draw_seat_image(Thickness margin)
        {
            Image myImage = new Image();
            int wide = 40;
            myImage.Width = wide;

            // Create source
            BitmapImage myBitmapImage = new BitmapImage();

            // BitmapImage.UriSource must be in a BeginInit/EndInit block
            myBitmapImage.BeginInit();
            string imageSource = @"C:\Users\GBX386\Desktop\Visual C#\WpfApplication2\WpfApplication2\Images\ChessMan\RedChess\ju.png";
            myBitmapImage.UriSource = new Uri(imageSource, UriKind.Absolute);

            // To save significant application memory, set the DecodePixelWidth or  
            // DecodePixelHeight of the BitmapImage value of the image source to the desired 
            // height or width of the rendered image. If you don't do this, the application will 
            // cache the image as though it were rendered as its normal size rather then just 
            // the size that is displayed.
            // Note: In order to preserve aspect ratio, set DecodePixelWidth
            // or DecodePixelHeight but not both.
            myBitmapImage.DecodePixelWidth = wide;
            myBitmapImage.EndInit();
            //set image source
            myImage.Source = myBitmapImage;
            myImage.HorizontalAlignment = HorizontalAlignment.Left;
            myImage.VerticalAlignment = VerticalAlignment.Top;
            myImage.Margin = margin;
            //myImage.MouseLeftButtonDown += new MouseButtonEventHandler(this.seat_mouse_lbtn_down);
            //myImage.MouseLeftButtonUp += new MouseButtonEventHandler(this.seat_mouse_lbtn_up);
            //myImage.MouseMove += new MouseEventHandler(this.myImage_MouseMove);

            gridChessBoard.Children.Add(myImage);


        }
        private void ChessBoardMouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition((IInputElement)sender);
            string locate = "unknown";
            string token = "unknown";

            switch (WpfApplication2.GameState.locate)
            {
                case 0:
                    locate = "Left"; break;
                case 1:
                    locate = "Right"; break;
                case 2:
                    locate = "Bottom"; break;
                default:
                    break;
            }
            
            switch ((Location)WpfApplication2.GameState.currentTokenLocate)
            {
                case Location.left:
                    token = "Left"; break;
                case Location.right:
                    token = "Right"; break;
                case Location.bottom:
                    token = "Bottom"; break;
                default:
                    break;
            }

            chessBoardPosition.Text = locate + " User,should " + token + " go! Point X:" + p.X + "  Y:" + p.Y;
        }

        public bool pressGameReadyBtn = false;
        private void StartGameReady(object sender, RoutedEventArgs e)
        {
            pressGameReadyBtn = true;
            
            if (GameState.locate == ChessBoard.firsr_come_user_locate)
            {
                TimeSetWin win = new TimeSetWin();
                win.Owner = this;
                win.ShowDialog();
                GamePlayingState state = new GamePlayingState();
                state.StartGameReadyReq(win.total_sec, win.step_sec);
            }
            else
            {
                GamePlayingState state = new GamePlayingState();
                state.StartGameReadyReq();
            }
            //ChessBoardLoaded(null, null);
        }

        private static uint message_count = 0;
        public void AddMsgToBox(string title, string msgContent)
        {
            Border border = new Border();
            if ((message_count++) % 2 == 0)
            {//left msg
                border.CornerRadius = new CornerRadius(15);
                border.HorizontalAlignment = HorizontalAlignment.Left;
                //border.Width = 
                border.Margin = new Thickness(3,3,20,3);
                border.Padding = new Thickness(1);
                
                GradientStopCollection gridCol = new GradientStopCollection();
                gridCol.Add(new GradientStop(Color.FromRgb(0x42, 0xc9, 0xff), 1));
                gridCol.Add(new GradientStop(Color.FromRgb(0x01, 0x48, 0xe3), 0));
                gridCol.Add(new GradientStop(Color.FromRgb(0x9d, 0xd1, 0xed), 0.583));

                LinearGradientBrush brush = new LinearGradientBrush(gridCol, new Point(0, 0), new Point(1, 0));
                border.Background = brush;

                //添加StackPanel
                StackPanel stack = new StackPanel();
                stack.Orientation = Orientation.Vertical;
                stack.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                stack.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

                TextBox userName = new TextBox();
                userName.IsReadOnly = true;
                userName.BorderThickness = new Thickness(0);
                userName.Background = Brushes.Transparent;
                userName.Text = title;

                TextBox content = new TextBox();
                content.IsReadOnly = true;
                content.BorderThickness = new Thickness(0);
                content.Background = Brushes.Transparent;
                content.Text = msgContent;
                content.TextWrapping = TextWrapping.Wrap;

                stack.Children.Add(userName);
                stack.Children.Add(content);

                ((System.Windows.Markup.IAddChild)border).AddChild(stack); 
            }
            else
            {
                border.CornerRadius = new CornerRadius(15);
                border.HorizontalAlignment = HorizontalAlignment.Right;
                //border.Width = 
                border.Margin = new Thickness(3, 3, 20, 3);
                border.Padding = new Thickness(1);

                GradientStopCollection gridCol = new GradientStopCollection();
                gridCol.Add(new GradientStop(Color.FromRgb(0x9f, 0xd9, 0xe5), 0.259));
                gridCol.Add(new GradientStop(Color.FromRgb(0x42, 0xc9, 0xff), 0.026));
                gridCol.Add(new GradientStop(Color.FromRgb(0x01, 0x48, 0xe3), 1));
                gridCol.Add(new GradientStop(Color.FromRgb(0x10, 0x88, 0xe3), 0.959));

                LinearGradientBrush brush = new LinearGradientBrush(gridCol, new Point(0, 0.5), new Point(1, 0.5));
                border.Background = brush;

                //添加StackPanel
                StackPanel stack = new StackPanel();
                stack.Orientation = Orientation.Vertical;
                stack.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                stack.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

                TextBox userName = new TextBox();
                userName.IsReadOnly = true;
                userName.BorderThickness = new Thickness(0);
                userName.Background = Brushes.Transparent;
                userName.Text = title;
                userName.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

                TextBox content = new TextBox();
                content.IsReadOnly = true;
                content.BorderThickness = new Thickness(0);
                content.Background = Brushes.Transparent;
                content.Text = msgContent;
                content.TextWrapping = TextWrapping.Wrap;

                stack.Children.Add(userName);
                stack.Children.Add(content);

                ((System.Windows.Markup.IAddChild)border).AddChild(stack); 
            }

            messageBoxStackPanel.Children.Add(border);

            messageBoxScro.ScrollToEnd();
            Chat_Click(null, null);
        }

        private void SendIMMsgBtn(object sender, RoutedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)comboBoxIMMsg.SelectedItem;
            GamePlayingState state = new GamePlayingState();

            if (ChessBoard.GetChessBoardObj() == null)
            {
                return;
            }

            if (item != null)
            {
                AddMsgToBox(ChessBoard.GetChessBoardObj().currentUser.user_name, item.Content.ToString());
                state.SendIMMessage(item.Content.ToString());
            }
            else
            {
                if (comboBoxIMMsg.Text.Length > 0)
                {
                    AddMsgToBox(ChessBoard.GetChessBoardObj().currentUser.user_name, comboBoxIMMsg.Text.ToString());
                    state.SendIMMessage(comboBoxIMMsg.Text.ToString());
                }
            }
        }

        public void DisplayTimer(int total, int single_step_time, Location locate)
        {
            int t_sec = total % 60;
            int t_min = total / 60;
            int s_sec = single_step_time % 60;
            int s_min = single_step_time / 60;

            switch(locate)
            {
                case Location.left:
                    //DispatchTimerLabLeft.Content = "Total: " + total.ToString() + " step: " + single_step_time.ToString();
                    DispatchTimerLeftTotalHour.Text = string.Format("{0,-2:D2}", t_min);
                    DispatchTimerLeftTotalMin.Text = string.Format("{0,-2:D2}", t_sec);

                    DispatchTimerLeftStepMin.Text = string.Format("{0,-2:D2}", s_min);
                    DispatchTimerLeftStepSec.Text = string.Format("{0,-2:D2}", s_sec);
                    break;
                case Location.right:
                    DispatchTimerRightTotalHour.Text = string.Format("{0,-2:D2}", t_min);
                    DispatchTimerRightTotalMin.Text = string.Format("{0,-2:D2}", t_sec);

                    DispatchTimerRightStepMin.Text = string.Format("{0,-2:D2}", s_min);
                    DispatchTimerRightStepSec.Text = string.Format("{0,-2:D2}", s_sec);
                    //DispatchTimerLabRight.Content = "Total: " + total.ToString() + " step: " + single_step_time.ToString();
                    break;
                case Location.bottom:
                    DispatchTimerBottomTotalHour.Text = string.Format("{0,-2:D2}", t_min);
                    DispatchTimerBottomTotalMin.Text = string.Format("{0,-2:D2}", t_sec);

                    DispatchTimerBottomStepMin.Text = string.Format("{0,-2:D2}", s_min);
                    DispatchTimerBottomStepSec.Text = string.Format("{0,-2:D2}", s_sec);
                    //DispatchTimerLabBottom.Content = "Total: " + total.ToString() + " step: " + single_step_time.ToString();
                    break;
                default:
                    break;
            }
        }

        public void TimeoutHandle(Location locate)
        {
            MessageBox.Show(locate.ToString() + " user has timeout");
        }

        private void ChessWindowActivedHand(object sender, EventArgs e)
        {
            Console.WriteLine("Chess Window Actived !! IsActive=" + this.IsActive);
            GameState.SetCurrentWin(this);
        }

        private void ChessWindowClosingHand(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (GameState.allUsersReady)
            {
                MessageBoxResult result = MessageBox.Show(this.Owner, "Getting out from the room, are you sure ?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    //handle something here, get out the game room!!!
                    GamePlayingState state = new GamePlayingState();
                    state.LeaveOutFromRoom();
                }
            }
            else
            {
                //handle something here, get out the game room!!!
                GamePlayingState state = new GamePlayingState();
                state.LeaveOutFromRoom();
            }
        }

        private void ChessBoardLoad(object sender, RoutedEventArgs e)
        {
            //gridChessBoard.Background;
            BitmapImage bit = new BitmapImage();
            bit.BeginInit();
            bit.UriSource = new Uri(@"C:\Users\GBX386\Desktop\Qipanxxx\drawable-mdpi\qiqi.png", UriKind.Absolute);
            bit.EndInit();
            
            ImageBrush image = new ImageBrush(bit);
            gridChessBoard.Background = image;
        }
        
        private void MouseLeftButtonDown_OpenUrl(object sender, MouseButtonEventArgs e)
        {
            // 打开一个链接
            AdvertisementImage image = sender as AdvertisementImage;
            string url = image.LinkUrl;
            if ((url != null) && (url.Length > 0))
            {
                System.Diagnostics.Process.Start(url);
            }
        }

        private void GetAdvertisementImage(int ImageMapIndex, ref Image image)
        {
            if (!ImageDownloadState.ImageIDMap[ImageMapIndex].existed)
            {
                return;
            }

            //Image image = null;
            string image_type = ImageDownloadState.ImageIDMap[ImageMapIndex].type;
            if (image_type.Equals("*.gif"))
            {
                AnimatedGIFControl gif = new AnimatedGIFControl(ImageDownloadState.ImageIDMap[ImageMapIndex].locate_path);
                gif.MouseLeftButtonDown += new MouseButtonEventHandler(MouseLeftButtonDown_OpenUrl);
                gif.Cursor = Cursors.Hand;
                gif.Stretch = Stretch.Uniform;
                gif.LinkUrl = ImageDownloadState.ImageIDMap[ImageMapIndex].link_url;
                gif.StartAnimate();
                image = gif;
            }
            else if (image_type.Equals("*.png") ||
                image_type.Equals("*.jpg") ||
                image_type.Equals("*.jpeg") ||
                image_type.Equals("*.bmp"))
            {
                BitmapImage myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.UriSource = new Uri(ImageDownloadState.ImageIDMap[ImageMapIndex].locate_path, UriKind.Absolute);
                myBitmapImage.EndInit();

                AdvertisementImage image_in = new AdvertisementImage();
                image_in.Source = myBitmapImage;
                image_in.Stretch = Stretch.Uniform;
                image_in.MouseLeftButtonDown += new MouseButtonEventHandler(MouseLeftButtonDown_OpenUrl);
                image_in.Cursor = Cursors.Hand;
                image_in.LinkUrl = ImageDownloadState.ImageIDMap[ImageMapIndex].link_url;
                image = image_in;
            }
        }
        
    }
}

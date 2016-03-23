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
    /// Interaction logic for Chess.xaml
    /// </summary>
    public partial class Chess : Window
    {
        private bool mRestoreIfMove = false;

        public Chess()
        {
            InitializeComponent();

            BitmapImage bt = new BitmapImage();
            bt.BeginInit();
            bt.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\MyIcon.png", UriKind.Absolute);
            bt.EndInit();
            this.Icon = bt;
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
                    break;
                case Location.bottom:
                    DispatchTimerBottomTotalHour.Text = string.Format("{0,-2:D2}", t_min);
                    DispatchTimerBottomTotalMin.Text = string.Format("{0,-2:D2}", t_sec);

                    DispatchTimerBottomStepMin.Text = string.Format("{0,-2:D2}", s_min);
                    DispatchTimerBottomStepSec.Text = string.Format("{0,-2:D2}", s_sec);
                    break;
                default:
                    break;
            }
        }

        public void TimeoutHandle(Location locate)
        {
            ChessBoard.GetChessBoardObj().gGameStatus = ChessBoard.GameSatus.END;
            GameState.allUsersReady = false;

            if ((int)locate == GameState.locate)
            {
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri(GameState.gWorkPath + @"\res\voice\gameover.wav", UriKind.Absolute));
                player.Play();
                
                ChessBoard.GetChessBoardObj().currentUser.State = User.GameState.LOSE;

                GamePlayingState state = new GamePlayingState();
                state.LeaveOutFromRoom();

                MessageBox.Show("走棋超时,本轮游戏结束！", "警告");
            }
        }

        private void ChessWindowActivedHand(object sender, EventArgs e)
        {
            Console.WriteLine("Chess Window Actived !! IsActive=" + this.IsActive);
            GameState.SetCurrentWin(this);
        }

        public void ChessWindowClosingHand(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (GameState.allUsersReady)
            {
                if ((ChessBoard.GetChessBoardObj().currentUser.State == User.GameState.PLAYING) &&
                    (ChessBoard.GetChessBoardObj().gGameStatus == ChessBoard.GameSatus.PLAYING))
                {
                    MessageBoxResult result = MessageBox.Show("确定要退出游戏吗(将被扣掉30分)?", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
            }

            if (!e.Cancel)
            {
                //handle something here, get out the game room!!!
                if (GameState.allUsersReady)
                {
                    ChessBoard.GetChessBoardObj().righttUser.timer.Stop();
                    ChessBoard.GetChessBoardObj().leftUser.timer.Stop();
                    ChessBoard.GetChessBoardObj().bottomUser.timer.Stop();
                    ChessBoard.GetChessBoardObj().currentUser.State = User.GameState.LOSE;
                }
                GameState.allUsersReady = false;
                GameState.gCurrUserGameStatus = UserStatus.STATUS_EXITED;
                ChessBoard.DestroryChessBoard();

                GamePlayingState state = new GamePlayingState();
                state.LeaveOutFromRoom("exit");
            }
        }

        private void ChessBoardLoad(object sender, RoutedEventArgs e)
        {
            //gridChessBoard.Background;
            BitmapImage bit = new BitmapImage();
            bit.BeginInit();
            bit.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\board.png", UriKind.Absolute);
            bit.EndInit();
            
            ImageBrush image = new ImageBrush(bit);
            gridChessBoard.Background = image;

            LoadChessBoadrMiddleAd();
            LoadChessBoadrBtRightAd();
        }

        public void SetStartButtonStatus(bool enable)
        {
            string url_locat = "";
            if (enable)
            {
                url_locat = GameState.gWorkPath + @"\res\Images\Button\StartEn.png";
                StartGameBtn.Cursor = Cursors.Hand;
            }
            else
            {
                url_locat = GameState.gWorkPath + @"\res\Images\Button\StartDis.png";
                StartGameBtn.Cursor = Cursors.Arrow;
            }

            BitmapImage bit = new BitmapImage();
            bit.BeginInit();
            bit.UriSource = new Uri(url_locat, UriKind.Absolute);
            bit.EndInit();

            StartGameBtn.Background = new ImageBrush(bit);
            StartGameBtn.IsEnabled = enable;
        }
        public void SetHuiQiButtonStatus(bool enable)
        {
            string url_locat = "";
            if (enable)
            {
                url_locat = GameState.gWorkPath + @"\res\Images\Button\huiQiEn.png";
                HuiQiBtn.Cursor = Cursors.Hand;
            }
            else
            {
                url_locat = GameState.gWorkPath + @"\res\Images\Button\huiQiDis.png"; 
                HuiQiBtn.Cursor = Cursors.Arrow;
            }

            BitmapImage bit = new BitmapImage();
            bit.BeginInit();
            bit.UriSource = new Uri(url_locat, UriKind.Absolute);
            bit.EndInit();

            HuiQiBtn.Background = new ImageBrush(bit);
            HuiQiBtn.IsEnabled = enable;
        }

        public void SetQiuHeButtonStatus(bool enable)
        {
            string url_locat = "";
            if (enable)
            {
                url_locat = GameState.gWorkPath + @"\res\Images\Button\qiuHeEn.png";
                HeQiBtn.Cursor = Cursors.Hand;
            }
            else
            {
                url_locat = GameState.gWorkPath + @"\res\Images\Button\qiuHeDis.png";
                HeQiBtn.Cursor = Cursors.Arrow;
            }

            BitmapImage bit = new BitmapImage();
            bit.BeginInit();
            bit.UriSource = new Uri(url_locat, UriKind.Absolute);
            bit.EndInit();

            HeQiBtn.Background = new ImageBrush(bit);
            HeQiBtn.IsEnabled = enable;
        }
        public void SetRenShuButtonStatus(bool enable)
        {
            string url_locat = "";
            if (enable)
            {
                url_locat = GameState.gWorkPath + @"\res\Images\Button\renShuEn.png";
                RenShuBtn.Cursor = Cursors.Hand;
            }
            else
            {
                url_locat = GameState.gWorkPath + @"\res\Images\Button\renShuDis.png";
                RenShuBtn.Cursor = Cursors.Arrow;
            }

            BitmapImage bit = new BitmapImage();
            bit.BeginInit();
            bit.UriSource = new Uri(url_locat, UriKind.Absolute);
            bit.EndInit();

            RenShuBtn.Background = new ImageBrush(bit);
            RenShuBtn.IsEnabled = enable;
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

        public void LoadChessBoadrMiddleAd()
        {
            Image image = null;
            GetAdvertisementImage((int)ImageID.IMAGE_ID_AD_BOARD_MIDDLE, ref image);
            if (image != null)
            {
                gridMiddleAd.Children.Add(image);
            }
        }

        public void ReMoveMiddleAd()
        {
            gridMiddleAd.Children.Clear();
        }

        
        public void LoadChessBoadrBtRightAd()
        {
            Image image = null;
            GetAdvertisementImage((int)ImageID.IMAGE_ID_AD_BOARD_BOTTOM_RIGHT, ref image);
            if (image != null)
            {
                gridBottomRightAd.Children.Add(image);
            }
        }

        public void ReMoveBottomRightAd()
        {
            gridBottomRightAd.Children.Clear();
        }

        public void DisplayHeadImage(Grid headgrid, byte []headImage)
        {
            BitmapImage b = null;
            try
            {
                b = new BitmapImage();
                b.BeginInit();
                b.StreamSource = new MemoryStream(headImage);
                b.EndInit();
            }
            catch
            {
                b = null;
                b = new BitmapImage();
                b.BeginInit();
                //默认头像
                b.UriSource = new Uri(GameState.defaultHeadImagePath, UriKind.Absolute);
                b.EndInit();
            }

            try
            {
                if ((headgrid != null) && (headgrid.Children.Count > 0))
                {
                    headgrid.Children.Clear();
                }

                Image image = new Image();
                image.Source = b;
                image.Stretch = Stretch.Uniform;
                image.Margin = new Thickness(15);
                headgrid.Children.Add(image);
            }
            catch (Exception e)
            {
                Console.WriteLine("Draw head image faile for " + e.Message);
            }
        }

        public void UsersInfoLoadAndUpdate()
        {
            LeftUserInfoLoad(null, null);
            BottomUserinfoLoad(null, null);
            RightUserInfoLoad(null, null);
        }

        public void LeftUserInfoLoad(object sender, RoutedEventArgs e)
        {
            if ((GameState.gLeftUser != null) && (!GameState.gLeftUser.ChessBoardEmpty))
            {
                DisplayHeadImage(LeftHeadImageGrid, GameState.gLeftUser.HeadImage.ToByteArray());
                LeftUserName.Text = GameState.gLeftUser.UserName;
                LeftUserScore.Text = GameState.gLeftUser.Score.ToString();
                LeftUserNameLab.Visibility = System.Windows.Visibility.Visible;
                LeftUserScoreLab.Visibility = System.Windows.Visibility.Visible;
                DispatchTimerLeftTotalHour.Visibility = System.Windows.Visibility.Visible;
                DispatchTimerLeftStepMin.Visibility = System.Windows.Visibility.Visible;
                DispatchTimerLeftTotalMin.Visibility = System.Windows.Visibility.Visible;
                DispatchTimerLeftStepSec.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                LeftHeadImageGrid.Children.Clear();
                LeftUserName.Text = "";
                LeftUserScore.Text = "";
                LeftUserNameLab.Visibility = System.Windows.Visibility.Hidden;
                LeftUserScoreLab.Visibility = System.Windows.Visibility.Hidden;
                DispatchTimerLeftTotalHour.Visibility = System.Windows.Visibility.Hidden;
                DispatchTimerLeftStepMin.Visibility = System.Windows.Visibility.Hidden;
                DispatchTimerLeftTotalMin.Visibility = System.Windows.Visibility.Hidden;
                DispatchTimerLeftStepSec.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        public void BottomUserinfoLoad(object sender, RoutedEventArgs e)
        {
            if ((GameState.gBottomUser != null) && (!GameState.gBottomUser.ChessBoardEmpty))
            {
                DisplayHeadImage(BottomHeadImageGrid, GameState.gBottomUser.HeadImage.ToByteArray());
                BottomUserName.Text = GameState.gBottomUser.UserName;
                BottomUserScore.Text = GameState.gBottomUser.Score.ToString();
                BottomUserNameLab.Visibility = System.Windows.Visibility.Visible;
                BottomUserScoreLab.Visibility = System.Windows.Visibility.Visible;
                DispatchTimerBottomTotalHour.Visibility = System.Windows.Visibility.Visible;
                DispatchTimerBottomStepMin.Visibility = System.Windows.Visibility.Visible;
                DispatchTimerBottomTotalMin.Visibility = System.Windows.Visibility.Visible;
                DispatchTimerBottomStepSec.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                BottomHeadImageGrid.Children.Clear();
                BottomUserName.Text = "";
                BottomUserScore.Text = "";
                BottomUserNameLab.Visibility = System.Windows.Visibility.Hidden;
                BottomUserScoreLab.Visibility = System.Windows.Visibility.Hidden;
                DispatchTimerBottomTotalHour.Visibility = System.Windows.Visibility.Hidden;
                DispatchTimerBottomStepMin.Visibility = System.Windows.Visibility.Hidden;
                DispatchTimerBottomTotalMin.Visibility = System.Windows.Visibility.Hidden;
                DispatchTimerBottomStepSec.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        public void RightUserInfoLoad(object sender, RoutedEventArgs e)
        {
            if ((GameState.gRightUser != null) && (!GameState.gRightUser.ChessBoardEmpty))
            {
                DisplayHeadImage(RightHeadImageGrid, GameState.gRightUser.HeadImage.ToByteArray());
                RightUserName.Text = GameState.gRightUser.UserName;
                RightUserScore.Text = GameState.gRightUser.Score.ToString();
                RightUserNameLab.Visibility = System.Windows.Visibility.Visible;
                RightUserScoreLab.Visibility = System.Windows.Visibility.Visible;
                DispatchTimerRightTotalHour.Visibility = System.Windows.Visibility.Visible;
                DispatchTimerRightStepMin.Visibility = System.Windows.Visibility.Visible;
                DispatchTimerRightTotalMin.Visibility = System.Windows.Visibility.Visible;
                DispatchTimerRightStepSec.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                RightHeadImageGrid.Children.Clear();
                RightUserName.Text = "";
                RightUserScore.Text = "";
                RightUserNameLab.Visibility = System.Windows.Visibility.Hidden;
                RightUserScoreLab.Visibility = System.Windows.Visibility.Hidden;
                DispatchTimerRightTotalHour.Visibility = System.Windows.Visibility.Hidden;
                DispatchTimerRightStepMin.Visibility = System.Windows.Visibility.Hidden;
                DispatchTimerRightTotalMin.Visibility = System.Windows.Visibility.Hidden;
                DispatchTimerRightStepSec.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void RequestHuiQi(object sender, RoutedEventArgs e)
        {
            GamePlayingState state = new GamePlayingState();
            state.UndoRequest();
            SetHuiQiButtonStatus(false);
        }

        private void QiuHeClick(object sender, RoutedEventArgs e)
        {
            GamePlayingState state = new GamePlayingState();
            state.QiuHeSendReq();
            SetQiuHeButtonStatus(false);
        }

        private void RenShuClick(object sender, RoutedEventArgs e)
        {
            if (ChessBoard.GetChessBoardObj().GetCurrentActiveUsrNum() < 3)
            {
                GamePlayingState state = new GamePlayingState();
                state.LeaveOutFromRoom();
                SetRenShuButtonStatus(false);
            }
        }

        private void toobar_grid_loaded(object sender, RoutedEventArgs e)
        {            //toobar_grid_loaded
            BitmapImage bt = new BitmapImage();
            bt.BeginInit();
            bt.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\MyIcon.png", UriKind.Absolute);
            bt.EndInit();

            Image image = new Image();
            image.Source = bt;
            image.Height = 20;
            image.Width = 20;
            image.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            image.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            image.Margin = new Thickness(5, 3, 10, 10);

            toobar_grid.Children.Add(image);
        }

        private void ChessContentLoad(object sender, RoutedEventArgs e)
        {
            BitmapImage bt = new BitmapImage();
            bt.BeginInit();
            bt.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\chat.png", UriKind.Absolute);
            bt.EndInit();
            chat_btn.Background = new ImageBrush(bt);

            bt = new BitmapImage();
            bt.BeginInit();
            bt.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\Button\startEn.png", UriKind.Absolute);
            bt.EndInit();
            StartGameBtn.Background = new ImageBrush(bt);

            bt = new BitmapImage();
            bt.BeginInit();
            bt.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\Button\huiQiDis.png", UriKind.Absolute);
            bt.EndInit();
            HuiQiBtn.Background = new ImageBrush(bt);

            bt = new BitmapImage();
            bt.BeginInit();
            bt.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\Button\qiuHeDis.png", UriKind.Absolute);
            bt.EndInit();
            HeQiBtn.Background = new ImageBrush(bt);

            bt = new BitmapImage();
            bt.BeginInit();
            bt.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\Button\renShuDis.png", UriKind.Absolute);
            bt.EndInit();
            RenShuBtn.Background = new ImageBrush(bt);

            bt = new BitmapImage();
            bt.BeginInit();
            bt.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\time_panel.png", UriKind.Absolute);
            bt.EndInit();
            leftTimepanel.Background = new ImageBrush(bt);

            bt = new BitmapImage();
            bt.BeginInit();
            bt.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\time_panel.png", UriKind.Absolute);
            bt.EndInit();
            rightTimepanel.Background = new ImageBrush(bt);

            bt = new BitmapImage();
            bt.BeginInit();
            bt.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\time_panel.png", UriKind.Absolute);
            bt.EndInit();
            bottomTimepanel.Background = new ImageBrush(bt);

            bt = new BitmapImage();
            bt.BeginInit();
            bt.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\background.png", UriKind.Absolute);
            bt.EndInit();
            chessContent.Background = new ImageBrush(bt);
        }

    }
}

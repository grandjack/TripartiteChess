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

using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 
    
    public partial class Window1 : Window
    {
        private bool mRestoreIfMove = false;

        public Window1()
        {
            InitializeComponent();
        }

        private void Close_Window(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Normal_Window(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;
                max_win.Visibility = Visibility.Collapsed;
                normal_win.Visibility = Visibility.Visible;
            }
            else if (this.WindowState != WindowState.Normal)
            {
                this.WindowState = WindowState.Normal;
                max_win.Visibility = Visibility.Visible;
                normal_win.Visibility = Visibility.Collapsed;
            }
        }

        private void Min_window(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                this.WindowState = WindowState.Minimized;
            }
        }

        private void Window_size_changed(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                max_win.Visibility = Visibility.Collapsed;
                normal_win.Visibility = Visibility.Visible;
                game_hall_loaded(null, null);
            }
            else if (this.WindowState == WindowState.Normal)
            {
                max_win.Visibility = Visibility.Visible;
                normal_win.Visibility = Visibility.Collapsed;
                game_hall_loaded(null, null);
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
        //自定义窗口实现最大化不覆盖任务栏--End
         
        private void list_select1(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                string str = listBox1.SelectedItem.ToString();
                string index = listBox1.SelectedIndex.ToString();
                str += " " + index;
                //MessageBox.Show(str);
                if (tab_game_hall.IsSelected != true)
                {
                    tab_game_hall.Visibility = Visibility.Visible;
                    tab_game_hall.IsSelected = true;
                }
                else
                {
                    tab_game_hall.Visibility = Visibility.Hidden;
                    tab_game_hall.IsSelected = false;
                    tab_game_describe.IsSelected = true;
                }
            }
        }

        private void GameHallSelectionChanged(object sender, SelectionChangedEventArgs e)
        {/*
            string str = listBox1.SelectedItem.ToString();
            string index = listBox1.SelectedIndex.ToString();
            str += " " + index;
            MessageBox.Show(str);*/
        }

        private void game_hall_loaded(object sender, RoutedEventArgs e)
        {
            grid_game_hall.Children.Clear();
            const int total_seats_num = 60;
            int total_row = 0;
            int total_column = 0;

            if (this.WindowState == WindowState.Normal)
            {
                total_column = 5;
                total_row = total_seats_num / total_column;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                total_column = 10;
                total_row = total_seats_num / total_column;
            }

            for (int row = 0; row < total_row; ++row)
            {
                for (int column = 0; column < total_column; ++column)
                {
                    Thickness margin = new Thickness(15 + column * 131, 15 + row * 131, 0, 0);
                    draw_seat_image(margin);
                }
            }
        }

        private void draw_seat_image(Thickness margin)
        {
            Image myImage = new Image();
            myImage.Width = 115;

            // Create source
            BitmapImage myBitmapImage = new BitmapImage();

            // BitmapImage.UriSource must be in a BeginInit/EndInit block
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(@"\Images\tablen.bmp", UriKind.Relative);

            // To save significant application memory, set the DecodePixelWidth or  
            // DecodePixelHeight of the BitmapImage value of the image source to the desired 
            // height or width of the rendered image. If you don't do this, the application will 
            // cache the image as though it were rendered as its normal size rather then just 
            // the size that is displayed.
            // Note: In order to preserve aspect ratio, set DecodePixelWidth
            // or DecodePixelHeight but not both.
            myBitmapImage.DecodePixelWidth = 115;
            myBitmapImage.EndInit();
            //set image source
            myImage.Source = myBitmapImage;
            myImage.HorizontalAlignment = HorizontalAlignment.Left;
            myImage.VerticalAlignment = VerticalAlignment.Top;
            myImage.Margin = margin;
            //myImage.MouseLeftButtonDown += new MouseButtonEventHandler(this.seat_mouse_lbtn_down);
            //myImage.MouseLeftButtonUp += new MouseButtonEventHandler(this.seat_mouse_lbtn_up);
            myImage.MouseMove += new MouseEventHandler(this.myImage_MouseMove);

            grid_game_hall.Children.Add(myImage);


        }

        private Border g_select_display_border = null;
        private Brush g_select_background = null;
        private void seat_mouse_lbtn_down(object sender, MouseButtonEventArgs e)
        {
            g_select_display_border = sender as Border;
            g_select_background = g_select_display_border.Background;
            g_select_display_border.Background = Brushes.Red;
        }

        private void seat_mouse_lbtn_up(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("Catch a lbtndown event!");
            Border border = sender as Border;

            g_select_display_border.Background = g_select_background;

            if (g_select_display_border != border)
            {
                return;
            }

            g_select_display_border = null;

            Chess chess = new Chess();
            chess.Show();
        }
        //private void icon_loaded(object sender, RoutedEventArgs e)
        private void icon_Initialized(object sender, EventArgs e)
        {
            // ... Create a new BitmapImage.
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri(@"Images/MyIcon.png",UriKind.Relative);
            b.EndInit();

            // ... Get Image reference from sender.
            var image = sender as Image;
            // ... Assign Source.
            image.Source = b;
        }

        //private void game_qipan_loaded(object sender, RoutedEventArgs e)
        private void game_qipan_loaded(object sender, EventArgs e)
        {
                        // ... Create a new BitmapImage.
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri(@"Images\QQiPan.png", UriKind.Relative);
            b.EndInit();

            // ... Get Image reference from sender.
            var image = sender as Image;
            // ... Assign Source.
            image.Source = b;
        }



        private Image current_mouse_over_image = null;
        private Border current_display_border = null;
        private void myImage_MouseMove(object sender, MouseEventArgs e)
        {
            Image myImage = sender as Image;
             

            Point p = e.GetPosition((IInputElement)sender);
            //log_describe.Text = "x:" + p.X + " y:" + p.Y;

            if (((p.X >= 37.5) && (p.Y >= 37.5)) && ((p.X <= 75) && (p.Y <= 75)))
            {
                if (current_mouse_over_image != myImage)
                {
                    //add rectangle
                    Border select_tag = new Border();
                    select_tag.HorizontalAlignment = HorizontalAlignment.Left;
                    select_tag.VerticalAlignment = VerticalAlignment.Top;
                    select_tag.Margin = new Thickness(myImage.Margin.Left + 35, myImage.Margin.Top + 35, myImage.Margin.Right, myImage.Margin.Bottom);
                    select_tag.Visibility = Visibility.Visible;
                    select_tag.Width = 44;
                    select_tag.Height = 44;
                    select_tag.Background = Brushes.Brown;
                    select_tag.CornerRadius = new CornerRadius(10);
                    select_tag.Opacity = 0.3;
                    select_tag.Cursor = Cursors.Hand;

                    select_tag.MouseLeftButtonDown += new MouseButtonEventHandler(this.seat_mouse_lbtn_down);
                    select_tag.MouseLeftButtonUp += new MouseButtonEventHandler(this.seat_mouse_lbtn_up);

                    current_display_border = select_tag;
                    current_mouse_over_image = myImage;
                    grid_game_hall.Children.Add(select_tag);
                    //grid_game_hall.Children.Remove(select_tag);
                }
            }
            else
            {
                if ((current_mouse_over_image != null) && (current_display_border != null))
                {
                    //if (g_select_display_border != current_display_border)
                    {
                        grid_game_hall.Children.Remove(current_display_border);
                        current_mouse_over_image = null;
                        current_display_border = null;
                    }
                }
            }

            if ( //(((p.X >= 37.5) && (p.Y >= 37.5)) && ((p.X <= 75) && (p.Y <= 75))) ||  //middle table
                 (((p.X >= 13) && (p.Y >= 45.6)) && ((p.X <= 33.2) && (p.Y <= 66.4))) || //left seat
                (((p.X >= 81) && (p.Y >= 45.6)) && ((p.X <= 103.2) && (p.Y <= 66.4))) )  //right seat
            {
                myImage.Cursor = Cursors.Hand;
            }
            else
            {
                myImage.Cursor = Cursors.Arrow;
            }
        }

        private void rec_ad_open(object sender, MouseButtonEventArgs e)
        {
            // 打开一个链接
            System.Diagnostics.Process.Start("http://www.baidu.com/");
        }

    }
}

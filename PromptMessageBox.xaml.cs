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

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for PromptMessageBox.xaml
    /// </summary>
    public partial class PromptMessageBox : Window
    {
        public PromptMessageBox(string title, string msg)
        {
            InitializeComponent();
            this.title_lab.Content = title;
            this.content_tx.Text = msg;
        }

        private void Close_Window(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }

    public class WindowShowTimer
    {
        private PromptMessageBox win = null;
        private Window own_win = null;
        private int time = 5;//seconds
        private string title = null;
        private string message = null;

        System.Windows.Threading.DispatcherTimer dispatcherTimer = null;

        public WindowShowTimer(Window own_win, string title, string message, int time)
        {
            this.time = time;
            this.title = title;
            this.message = message;
            this.own_win = own_win;

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            if (this.time == -1)
            {
                this.time = 60 * 60;
            }

            dispatcherTimer.Interval = new TimeSpan(0, 0, this.time);
        }
        
        public WindowShowTimer(Window own_win, string title, string message)
        {
            this.title = title;
            this.message = message;
            this.own_win = own_win;

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, this.time);
        }

        public void Show()
        {
            try
            {
                this.Start();
                win = new PromptMessageBox(title, message);
                win.Owner = own_win;
                win.Show();
            }
            catch (Exception e)
            {
                Console.WriteLine("Can NOT show box for " + e.Message);
            }
        }


        public void Start()
        {
            dispatcherTimer.Start();
        }

        public void Stop()
        {
            dispatcherTimer.Stop();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            win.Close();
            CommandManager.InvalidateRequerySuggested();
            return;
        }
    }

    public class DisplayBingTips
    {
        private byte row = 0;
        private byte column = 0;
        private int timeInterval = 0;
        private System.Windows.Threading.DispatcherTimer dispatcherTimer = null;
        static private DisplayBingTips tips = null;

        public DisplayBingTips(byte row, byte column, int time = 2)
        {
            this.timeInterval = time;
            this.row = row;
            this.column = column;

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            dispatcherTimer.Interval = new TimeSpan(0, 0, this.timeInterval);
        }

        public void Start()
        {
            dispatcherTimer.Start();
        }

        public void Stop()
        {
            dispatcherTimer.Stop();
        }
        
        private string GetImgViaTipDirection(User owner,
            ChessMan.MoveEvent moveEventUp,
            ChessMan.MoveEvent moveEventDown,
            ChessMan.MoveEvent moveEventLeft,
            ChessMan.MoveEvent moveEventRight)
        {
            Console.WriteLine("moveEventUp:" + moveEventUp);
            Console.WriteLine("moveEventDown:" + moveEventDown);
            Console.WriteLine("moveEventLeft:" + moveEventLeft);
            Console.WriteLine("moveEventRight:" + moveEventRight);

            string imgFile = "";
            switch (owner.GetUserLocation())
            {
                case Location.left:
                    imgFile = "Black";
                    if (moveEventLeft == ChessMan.MoveEvent.INVALID_MOVE_EVENT)
                    {
                        if (moveEventUp == ChessMan.MoveEvent.INVALID_MOVE_EVENT)
                        {
                            imgFile += "Right";
                        }
                        else{
                            imgFile += "NotLeft";
                        }
                    }
                    else//说明在下方
                    {
                        imgFile += "NotUp";
                    }
                    break;

                case Location.right:
                    imgFile = "Green";
                    if (moveEventRight == ChessMan.MoveEvent.INVALID_MOVE_EVENT)
                    {
                        if (moveEventUp == ChessMan.MoveEvent.INVALID_MOVE_EVENT)
                        {
                            imgFile += "Left";
                        }
                        else
                        {
                            imgFile += "NotRight";
                        }
                    }
                    else
                    {
                        imgFile += "NotUp";
                    }
                    break;

                case Location.bottom:
                    imgFile = "Red";
                    if ((moveEventRight != ChessMan.MoveEvent.INVALID_MOVE_EVENT) &&
                        (moveEventLeft != ChessMan.MoveEvent.INVALID_MOVE_EVENT))
                    {
                        imgFile += "NotDown";
                    }
                    else if ((moveEventUp != ChessMan.MoveEvent.INVALID_MOVE_EVENT) &&
                        (moveEventDown == ChessMan.MoveEvent.INVALID_MOVE_EVENT))
                    {
                        imgFile += "Up";
                    }
                    else if (moveEventLeft != ChessMan.MoveEvent.INVALID_MOVE_EVENT)
                    {
                        imgFile += "NotRight";
                    }
                    else
                    {
                        imgFile += "NotLeft";
                    }
                    break;

                default:
                    break;
            }

            imgFile += ".png";

            Console.WriteLine("Current Image : " + imgFile);

            return (GameState.gWorkPath + @"\res\Images\Arrows\" + imgFile);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if ((ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column].chessGrid != ChessBoard.targetGrid) &&
                (ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column].chessGrid != ChessBoard.selectGrid))
            {
                ChessMan chessMan = ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column].chess;
                //检测上、下、左、右四个方向可行性
                ChessMan.MoveEvent moveEventUp = chessMan.CheckMoveEvent((byte)(chessMan.Row - 1), chessMan.Column);
                ChessMan.MoveEvent moveEventDown = chessMan.CheckMoveEvent((byte)(chessMan.Row + 1), chessMan.Column);
                ChessMan.MoveEvent moveEventLeft = chessMan.CheckMoveEvent(chessMan.Row, (byte)(chessMan.Column - 1));
                ChessMan.MoveEvent moveEventRight = chessMan.CheckMoveEvent(chessMan.Row, (byte)(chessMan.Column + 1));

                string pathFile = GetImgViaTipDirection(chessMan.GetOwnUser(), moveEventUp, moveEventDown, moveEventLeft, moveEventRight);

                BitmapImage myBitmapImage = new BitmapImage();
                // BitmapImage.UriSource must be in a BeginInit/EndInit block
                myBitmapImage.BeginInit();
                myBitmapImage.UriSource = new Uri(pathFile, UriKind.Absolute);
                myBitmapImage.EndInit();

                ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column].chessGrid.Background = new ImageBrush(myBitmapImage);

                ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column].chess.Margin = new Thickness(4);
            }

            //display the background of grid
            CommandManager.InvalidateRequerySuggested();
            return;
        }

        static public void DisplayBingTip(byte row, byte column)
        {
            tips = new DisplayBingTips(row, column, 1);
            tips.Start();
        }

        static public void DestroryBingTip()
        {
            if (tips != null)
            {
                tips.Stop();

                if ((ChessBoard.GetChessBoardObj().g_chess_board[tips.row, tips.column].chessGrid != ChessBoard.targetGrid) &&
                    (ChessBoard.GetChessBoardObj().g_chess_board[tips.row, tips.column].chessGrid != ChessBoard.selectGrid))
                {
                    if (ChessBoard.GetChessBoardObj().g_chess_board[tips.row, tips.column].chessGrid.Background != Brushes.Transparent)
                    {
                        ChessBoard.GetChessBoardObj().g_chess_board[tips.row, tips.column].chessGrid.Background = Brushes.Transparent;
                        ChessBoard.GetChessBoardObj().g_chess_board[tips.row, tips.column].chess.Margin = new Thickness(0);
                    }
                }

                tips = null;
            }
        }
    }
}

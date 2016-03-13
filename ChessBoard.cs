using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApplication2
{
    struct GridPoint
    {
        public byte row;
        public byte colomn;
    };

    class ChessBoard
    {
        public const byte BOARD_ROW_NUM = 14;
        public const byte BOARD_COLUMN_NUM = 19;
        public enum BoardStatus { illegal, empty, occupied }

        private static ChessBoard chessBoard = null;

        public struct BoardObject
        {
            public BoardStatus status;
            public ChessMan chess;
        }

        public BoardObject[,] g_chess_board = new BoardObject[BOARD_ROW_NUM, BOARD_COLUMN_NUM];

        public User leftUser = null;
        public User righttUser = null;
        public User bottomUser = null;
        public User currentUser = null;

        public Location currUserLocation = Location.unknown;
        public Location currPriority = Location.bottom;

        public static Chess chessWindow = null;
        public ChessMan currSelectChess = null;
        public static UIElement selectElement = null;
        public static UIElement targetElement = null;

        public static int total_time = 0;
        public static int single_step_time = 0;
        public static int firsr_come_user_locate = (int)Location.unknown;

        public Location CurrentPriority
        {
            get { return this.currPriority; }
            set { this.currPriority = value; }
        }

        public static void handler_chess_move(int fromX, int fromY, int desX, int desY)
        {
            //必须先要释放之前的选择标识
            if (ChessBoard.selectElement != null)
            {
                ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.selectElement);
                ChessBoard.selectElement = null;
            }
            if (ChessBoard.targetElement != null)
            {
                ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.targetElement);
                ChessBoard.targetElement = null;
            }

            GetChessBoardObj().currSelectChess = null;

            ChessMan chessMan = chessBoard.g_chess_board[fromX, fromY].chess;
            if (chessMan != null)
            {
                Thickness srcThinc = new Thickness(chessMan.Margin.Left, chessMan.Margin.Top - 1, chessMan.Margin.Right, chessMan.Margin.Bottom);
                chessMan.beSelected = true;
                if (chessMan.MoveChess((byte)desX, (byte)desY, false))
                {

                    Thickness desThinc = new Thickness(chessMan.Margin.Left, chessMan.Margin.Top - 1, chessMan.Margin.Right, chessMan.Margin.Bottom);

                    //添加两个选择标识
                    ChessBoard.selectElement = SetSelectTagImg(srcThinc, (int)chessMan.Width);
                    ChessBoard.chessWindow.gridChessBoard.Children.Add(ChessBoard.selectElement);

                    ChessBoard.targetElement = SetSelectTagImg(desThinc, (int)chessMan.Width);
                    ChessBoard.chessWindow.gridChessBoard.Children.Add(ChessBoard.targetElement);
                }
                else
                {
                    Console.WriteLine("MoveChess failed!");
                }
            }
            else
            {
                Console.WriteLine("Can NOT get the original chess!");
            }
        }

        public static void gridChessBoard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition((IInputElement)sender);
            GridPoint point = GetGridPointByPoint(p);
            chessWindow.chessBoardPosition.Text = "Click Down X:" + point.row + "  Y:" + point.colomn;

            if (!WpfApplication2.GameState.allUsersReady || (WpfApplication2.GameState.currentTokenLocate != (Location)WpfApplication2.GameState.locate))
            {
                MessageBox.Show("You should NOT go!!");
                //play the invalid tone
                return;
            }

            ChessMan chessMan = chessBoard.currSelectChess;

            if (chessBoard.currSelectChess != null)
            {
                if (true == chessBoard.currSelectChess.MoveChess(point.row, point.colomn))
                {
                    chessBoard.currSelectChess.beSelected = false;
                    chessBoard.currSelectChess.Opacity = 1;
                    chessBoard.currSelectChess = null;
                    GameState.currentTokenLocate = Location.unknown;

                    if (ChessBoard.targetElement != null)
                    {
                        Image select_tag = ChessBoard.targetElement as Image;
                        select_tag.Margin = new Thickness(chessMan.Margin.Left, chessMan.Margin.Top - 1, chessMan.Margin.Right, chessMan.Margin.Bottom);

                        ChessBoard.chessWindow.gridChessBoard.Children.Add(ChessBoard.targetElement);
                    }
                    else
                    {
                        ChessBoard.targetElement = SetSelectTagImg(new Thickness(chessMan.Margin.Left, chessMan.Margin.Top - 1, chessMan.Margin.Right, chessMan.Margin.Bottom), (int)chessMan.Width);
                        ChessBoard.chessWindow.gridChessBoard.Children.Add(ChessBoard.targetElement);
                    }
                }
            }
            else
            {
                if (ChessBoard.selectElement != null)
                {
                    ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.selectElement);
                }
                if (ChessBoard.targetElement != null)
                {
                    ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.targetElement);
                }
            }
        }

        public static Image SetSelectTagImg(Thickness thick, int width)
        {
            Image select_tag = new Image();
            select_tag.HorizontalAlignment = HorizontalAlignment.Left;
            select_tag.VerticalAlignment = VerticalAlignment.Top;
            select_tag.Margin = thick;
            select_tag.Visibility = Visibility.Visible;
            select_tag.Width = width - 1;
            select_tag.Height = width - 1;

            // Create source
            BitmapImage myBitmapImage = new BitmapImage();

            // BitmapImage.UriSource must be in a BeginInit/EndInit block
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(@"C:\Users\GBX386\Desktop\Visual C#\WpfApplication2\WpfApplication2\Images\selected.png", UriKind.Absolute);

            myBitmapImage.DecodePixelWidth = width;
            myBitmapImage.EndInit();
            //set image source
            select_tag.Source = myBitmapImage;

            select_tag.Opacity = 1;

            //ChessBoard.chessWindow.gridChessBoard.Children.Add(select_tag);

            return select_tag;
        }

        public static Point GetPointByGrid(byte row, byte column)
        {
            Point p = new Point();
            p.X = 2 + column * 50.4;
            p.Y = 2 + row * 50.4;
            return p;
        }

        public static GridPoint GetGridPointByPoint(Point p)
        {
            GridPoint P = new GridPoint();
            byte tmpmix = 0, tmpmax = 0;
            byte tmpmiy = 0, tmpmay = 0;

            tmpmix = (byte)((p.Y - 22) / 50.4);
            tmpmax = (byte)((p.Y - 2) / 50.4);
            if (tmpmix == tmpmax || tmpmix + 1 == tmpmax)
            {
                P.row = tmpmax;
            }
            else
            {
                MessageBox.Show("Calculate wrong point!");
            }

            tmpmiy = (byte)((p.X - 22) / 50.4);
            tmpmay = (byte)((p.X - 2) / 50.4);
            if (tmpmiy == tmpmay || tmpmiy + 1 == tmpmay)
            {
                P.colomn = tmpmay;
            }
            else
            {
                MessageBox.Show("Calculate wrong point!");
            }

            return P;
        }

        public static ChessBoard GetChessBoardObj(Location currUserLocation = Location.unknown, Chess chessWin=null)
        {
            if (chessBoard == null)
            {
                if (currUserLocation != Location.unknown)
                {
                    chessWindow = chessWin;
                    chessWindow.gridChessBoard.MouseLeftButtonDown += new MouseButtonEventHandler(gridChessBoard_MouseLeftButtonDown);
                    chessBoard = new ChessBoard(currUserLocation);
                }
            }

            return chessBoard;
        }

        private ChessBoard(Location currUserLocation)
        {
            //Initialize chess board must before create the Users
            for (byte i = 0; i < BOARD_ROW_NUM; ++i)
            {
                for (byte j = 0; j < BOARD_COLUMN_NUM; ++j)
                {
                    this.g_chess_board[i, j].chess = null;

                    if ((i > 8) && (j < 5 || j > 13))
                        this.g_chess_board[i, j].status = BoardStatus.illegal;
                    else
                        this.g_chess_board[i, j].status = BoardStatus.empty;
                }
            }

            this.currUserLocation = currUserLocation;
            this.InitailUsersInfo();
        }

        private void InitailUsersInfo()
        {
            //create users
            this.leftUser = new LeftUser(this);
            this.righttUser = new RightUser(this);
            this.bottomUser = new BottomUser(this);

            switch (this.currUserLocation)
            {
                case Location.left:
                    currentUser = leftUser;
                    break;
                case Location.right:
                    currentUser = righttUser;
                    break;
                case Location.bottom:
                    currentUser = bottomUser;
                    break;
                default:
                    break;
            }

        }

        public static bool IsValidPoint(int row, int column)
        {
            if (row < 0 || row >= BOARD_ROW_NUM ||
                column < 0 || column >= BOARD_COLUMN_NUM ||
                GetChessBoardObj().g_chess_board[row, column].status == BoardStatus.illegal)
            {
                return false;
            }

            return true;
        }

        public void PrintChessBoard()
        {
            Console.Write("\\\t");

            for (byte j = 0; j < BOARD_COLUMN_NUM; ++j)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("{0}\t", j);
            }
            Console.Write("\n");

            for (byte i = 0; i < BOARD_ROW_NUM; ++i)
            {
                for (byte j = 0; j < BOARD_COLUMN_NUM; ++j)
                {
                    if (j == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("{0}\t", i);
                    }

                    if (this.g_chess_board[i, j].status == BoardStatus.illegal)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("X\t");
                    }
                    else if (this.g_chess_board[i, j].status == BoardStatus.empty)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("O\t");
                    }
                    else
                    {
                        try
                        {
                            if (this.g_chess_board[i, j].chess.GetOwnUser().GetUserLocation() == Location.left)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                            }
                            else if (this.g_chess_board[i, j].chess.GetOwnUser().GetUserLocation() == Location.right)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                            }

                            Console.Write("{0}\t", this.g_chess_board[i, j].chess.GetChessName());
                        }
                        catch
                        {
                            Console.WriteLine("Got image chessMan failed");
                        }
                    }

                    if (j == BOARD_COLUMN_NUM - 1)
                        Console.Write("\n\n\n");
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public ChessMan GetSpecOneImage(int row, int column)
        {
            if (!IsValidPoint(row, column))
            {
                Console.WriteLine("Invalid Point!");
                return null;
            }

            if (this.g_chess_board[row, column].status == BoardStatus.empty)
            {
                Console.WriteLine("Empty Point!");
                return null;
            }

            return this.g_chess_board[row, column].chess;
        }

        public ChessMan GetSpecOneChessByType(User owner, ChessMan.ChessSpec type)
        {
            if (owner == null || type < 0 || type >= ChessMan.ChessSpec.CHESS_NUM)
            {
                return null;
            }

            for (byte i = 0; i < BOARD_ROW_NUM; ++i)
            {
                for (byte j = 0; j < BOARD_COLUMN_NUM; ++j)
                {
                    if ((this.g_chess_board[i, j].status == BoardStatus.occupied) && (this.g_chess_board[i, j].chess != null))
                    {
                        if ((this.g_chess_board[i, j].chess.GetOwnUser() == owner) && (this.g_chess_board[i, j].chess.GetChessType() == type))
                        {
                            return this.g_chess_board[i, j].chess;
                        }
                    }
                }
            }

            return null;
        }

        public void UpdateChessOwner(User originOwner, User newOwner)
        {
            if ((originOwner == null) || (newOwner == null))
            {
                return;
            }

            for (byte i = 0; i < BOARD_ROW_NUM; ++i)
            {
                for (byte j = 0; j < BOARD_COLUMN_NUM; ++j)
                {
                    if ((this.g_chess_board[i, j].status == BoardStatus.occupied) && (this.g_chess_board[i, j].chess != null))
                    {
                        if (this.g_chess_board[i, j].chess.GetOwnUser() == originOwner)
                        {
                            ChessMan chess = this.g_chess_board[i, j].chess;
                            chess.SetOwnUser(newOwner);
                            chess.InitialImageUriSource(chess.GetChessName());

                            // Create source
                            BitmapImage myBitmapImage = new BitmapImage();
                            myBitmapImage.BeginInit();
                            myBitmapImage.UriSource = new Uri(chess.imageUriSource, UriKind.Absolute);

                            myBitmapImage.DecodePixelWidth = (int)chess.Width;
                            myBitmapImage.EndInit();
                            //set image source
                            chess.Source = myBitmapImage;
                        }
                    }
                }
            }
        }

        public bool CheckIfJiangJun(User curActiveUsr, User passiveUsr)
        {
            if ((curActiveUsr == null) || (passiveUsr == null) || (passiveUsr.State != User.GameState.PLAYING))
            {
                return false;
            }

            ChessMan passiveCaptain = this.GetSpecOneChessByType(passiveUsr, ChessMan.ChessSpec.CHESS_SHUAI);
            if (passiveCaptain == null)
            {
                return false;
            }

            for (byte i = 0; i < BOARD_ROW_NUM; ++i)
            {
                for (byte j = 0; j < BOARD_COLUMN_NUM; ++j)
                {
                    if ((this.g_chess_board[i, j].status == BoardStatus.occupied) &&
                        (this.g_chess_board[i, j].chess != null) &&
                        (this.g_chess_board[i, j].chess.GetOwnUser() == curActiveUsr))
                    {
                        if (ChessMan.MoveEvent.MOVE_TO_EAT_EVENT == this.g_chess_board[i, j].chess.CheckMoveEvent(passiveCaptain.Row, passiveCaptain.Column))
                            return true;
                    }
                }
            }

            return false;
        }

        public byte GetCurrentActiveUsrNum()
        {
            byte num = 0;

            if ((leftUser != null) && (leftUser.State != User.GameState.LOSE))
            {
                ++num;
            }

            if ((righttUser != null) && (righttUser.State != User.GameState.LOSE))
            {
                ++num;
            }

            if ((bottomUser != null) && (bottomUser.State != User.GameState.LOSE))
            {
                ++num;
            }

            return num;
        }

        public User GetUserByUsrLocation(Location location)
        {
            if (location == Location.left)
                return leftUser;
            else if (location == Location.right)
                return righttUser;
            else if (location == Location.bottom)
                return bottomUser;

            return null;
        }

        public static Location GetPointLocation(byte row, byte column)
        {
            if (!ChessBoard.IsValidPoint(row, column))
            {
                return Location.unknown;
            }

            if ((row >= 0) && (row <= 8) && (column >= 0) && (column <= 4))
            {
                return Location.left;
            }
            else if ((row >= 0) && (row <= 8) && (column >= 14) && (column < ChessBoard.BOARD_COLUMN_NUM))
            {
                return Location.right;
            }
            else if ((row >= 9) && (row < ChessBoard.BOARD_ROW_NUM) && (column >= 5) && (column <= 13))
            {
                return Location.bottom;
            }
            else
            {
                return Location.unknown;
            }
        }

        public static bool CheckIfPublicArea(byte row, byte column)
        {
            bool ret = false;
            Location location = ChessBoard.GetPointLocation(row, column);
            switch (location)
            {
                case Location.left:
                    if (ChessBoard.GetChessBoardObj().leftUser.State == User.GameState.LOSE)
                    {
                        ret = true;
                    }
                    break;

                case Location.right:
                    if (ChessBoard.GetChessBoardObj().righttUser.State == User.GameState.LOSE)
                    {
                        ret = true;
                    }
                    break;

                case Location.bottom:
                    if (ChessBoard.GetChessBoardObj().bottomUser.State == User.GameState.LOSE)
                    {
                        ret = true;
                    }
                    break;

                //case Location.middle:
                 //   ret = true;
                 //   break;

                default:
                    break;
            }

            return ret;
        }

        //检测士、帅、象所属合法区域
        public static bool ValidChessManScope(ChessMan.ChessSpec chessType, byte d_row, byte d_column)
        {
            switch (chessType)
            {
                case ChessMan.ChessSpec.CHESS_SHUAI:
                case ChessMan.ChessSpec.CHESS_SHI:
                    if ((d_row >= 3) && (d_row <= 5) &&
                       (d_column >= 0) && (d_column <= 2))
                    {
                        return true;
                    }
                    else if (d_row >= 3 && d_row <= 5 &&
                            d_column >= 16 && d_column < ChessBoard.BOARD_COLUMN_NUM)
                    {
                        return true;
                    }
                    else if (d_row >= 11 && d_row <= 13 &&
                        d_column >= 8 && d_column <= 10)
                    {
                        return true;
                    }
                    break;

                case ChessMan.ChessSpec.CHESS_XIANG:
                    if (d_row >= 0 && d_row <= 8 &&
                        d_column >= 0 && d_column <= 4)
                    {
                        return true;
                    }
                    else if (d_row >= 0 && d_row <= 8 &&
                        d_column >= 14 && d_column < ChessBoard.BOARD_COLUMN_NUM)
                    {
                        return true;
                    }
                    else if (d_row >= 9 && d_row <= 13 &&
                        d_column >= 5 && d_column <= 13)
                    {
                        return true;
                    }
                    break;

                default:
                    break;
            }

            return false;
        }
    }
}

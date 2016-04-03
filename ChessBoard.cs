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
    class RegretChess
    {
        public byte from_x;
        public byte from_y;
        public byte des_x;
        public byte des_y;
        public bool ate_chess = false;
        public ChessMan chess;
    }

    class ChessBoard
    {
        public const byte BOARD_ROW_NUM = 14;
        public const byte BOARD_COLUMN_NUM = 19;
        public enum BoardStatus { illegal, empty, occupied }
        public enum GameSatus { NOT_START=0, END = NOT_START, PLAYING}

        private static ChessBoard chessBoard = null;

        public GameSatus gGameStatus = GameSatus.NOT_START;

        public struct BoardObject
        {
            public BoardStatus status;
            public ChessMan chess;
            public Grid chessGrid;
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

        private static Grid selectGrid = null;
        private static Grid targetGrid = null;

        public static int total_time = 0;
        public static int single_step_time = 0;
        public static int firsr_come_user_locate = (int)Location.unknown;

        public static RegretChess gHuiQiStack = new RegretChess();
        //用以 标识是否可以悔棋
        public static bool justGo = false;

        public Location CurrentPriority
        {
            get { return this.currPriority; }
            set { this.currPriority = value; }
        }

        public static void handler_chess_move(int fromX, int fromY, int desX, int desY)
        {
            //必须先要释放之前的选择标识
            if (ChessBoard.IsSelected())
            {
                ChessBoard.SetSelectGrid(false);
            }
            if (ChessBoard.IsTargeted())
            {
                ChessBoard.SetTargetGrid(false);
            }

            //此时禁止用户再悔棋，因为其他对手已经开始走子
            if (ChessBoard.justGo)
            {
                ChessBoard.justGo = false;
                GameState.gameWin.SetHuiQiButtonStatus(false);
            }

            GetChessBoardObj().currSelectChess = null;

            ChessMan chessMan = chessBoard.g_chess_board[fromX, fromY].chess;
            if (chessMan != null)
            {
                chessMan.beSelected = true;
                if (chessMan.MoveChess((byte)desX, (byte)desY, false))
                {
                    chessMan.beSelected = false;
                    //添加两个选择标识
                    ChessBoard.SetSelectGrid(true, ChessBoard.GetChessBoardObj().g_chess_board[fromX, fromY].chessGrid);

                    ChessBoard.SetTargetGrid(true, ChessBoard.GetChessBoardObj().g_chess_board[desX, desY].chessGrid);
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

        public static void HuiQi_chess_move()
        {
            //必须先要释放之前的选择标识
            if (ChessBoard.IsSelected())
            {
                ChessBoard.SetSelectGrid(false);
            }
            if (ChessBoard.IsTargeted())
            {
                ChessBoard.SetTargetGrid(false);
            }

            GetChessBoardObj().currSelectChess = null;


            chessBoard.g_chess_board[gHuiQiStack.from_x, gHuiQiStack.from_y].chess = chessBoard.g_chess_board[gHuiQiStack.des_x, gHuiQiStack.des_y].chess;
            chessBoard.g_chess_board[gHuiQiStack.from_x, gHuiQiStack.from_y].chess.Row = gHuiQiStack.from_x;
            chessBoard.g_chess_board[gHuiQiStack.from_x, gHuiQiStack.from_y].chess.Column = gHuiQiStack.from_y;
            chessBoard.g_chess_board[gHuiQiStack.from_x, gHuiQiStack.from_y].status = BoardStatus.occupied;
            chessBoard.g_chess_board[gHuiQiStack.from_x, gHuiQiStack.from_y].chessGrid.Children.Clear();
            chessBoard.g_chess_board[gHuiQiStack.des_x, gHuiQiStack.des_y].chessGrid.Children.Clear();
            chessBoard.g_chess_board[gHuiQiStack.from_x, gHuiQiStack.from_y].chessGrid.Children.Add(chessBoard.g_chess_board[gHuiQiStack.from_x, gHuiQiStack.from_y].chess);

            if (gHuiQiStack.ate_chess)
            {
                if (gHuiQiStack.chess.GetChessType() == ChessMan.ChessSpec.CHESS_SHUAI)
                {
                    Console.WriteLine("Current user:" + chessBoard.g_chess_board[gHuiQiStack.from_x, gHuiQiStack.from_y].chess.GetOwnUser().ToString());
                    Console.WriteLine("Original user:" + gHuiQiStack.chess.GetOriginalOwnUser());
                    chessBoard.BackToOriginalOwner(chessBoard.g_chess_board[gHuiQiStack.from_x, gHuiQiStack.from_y].chess.GetOwnUser(),
                        gHuiQiStack.chess.GetOriginalOwnUser());
                    //更新用户状态
                    gHuiQiStack.chess.GetOriginalOwnUser().State = User.GameState.PLAYING;
                }

                chessBoard.g_chess_board[gHuiQiStack.des_x, gHuiQiStack.des_y].chess = gHuiQiStack.chess;
                chessBoard.g_chess_board[gHuiQiStack.des_x, gHuiQiStack.des_y].status = BoardStatus.occupied;
                chessBoard.g_chess_board[gHuiQiStack.des_x, gHuiQiStack.des_y].chess.Row = gHuiQiStack.des_x;
                chessBoard.g_chess_board[gHuiQiStack.des_x, gHuiQiStack.des_y].chess.Column = gHuiQiStack.des_y;
                chessBoard.g_chess_board[gHuiQiStack.des_x, gHuiQiStack.des_y].chessGrid.Children.Add(chessBoard.g_chess_board[gHuiQiStack.des_x, gHuiQiStack.des_y].chess);
            }
            else
            {
                chessBoard.g_chess_board[gHuiQiStack.des_x, gHuiQiStack.des_y].chess = null;
                chessBoard.g_chess_board[gHuiQiStack.des_x, gHuiQiStack.des_y].status = BoardStatus.empty;
                chessBoard.g_chess_board[gHuiQiStack.des_x, gHuiQiStack.des_y].chessGrid.Children.Clear();
            }

            //添加两个选择标识
            ChessBoard.SetSelectGrid(true, ChessBoard.GetChessBoardObj().g_chess_board[gHuiQiStack.from_x, gHuiQiStack.from_y].chessGrid);

            ChessBoard.SetTargetGrid(true, ChessBoard.GetChessBoardObj().g_chess_board[gHuiQiStack.des_x, gHuiQiStack.des_y].chessGrid);

            //重新设置当前棋面状态
            GameState.currentTokenLocate = (Location)GamePlayingState.gUndo_msg.SrcUserLocate;

            User user = ChessBoard.GetChessBoardObj().GetUserByUsrLocation((Location)GamePlayingState.gUndo_msg.TarUserLocate);
            if (user != null)
            {
                user.timer.Stop();
            }

            user = ChessBoard.GetChessBoardObj().GetUserByUsrLocation((Location)GamePlayingState.gUndo_msg.SrcUserLocate);
            if (user != null)
            {
                user.timer.Start();
            }

        }

        public static void gridChessBoard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Grid chessGrid = sender as Grid;
            byte des_row = (byte)Grid.GetRow(chessGrid);
            byte des_colomn = (byte)Grid.GetColumn(chessGrid);
            e.Handled = true;
            
            if (!WpfApplication2.GameState.allUsersReady || (WpfApplication2.GameState.currentTokenLocate != (Location)WpfApplication2.GameState.locate))
            {
                return;
            }

            if (ChessBoard.GetChessBoardObj().GetUserByUsrLocation((Location)WpfApplication2.GameState.locate).State != User.GameState.PLAYING)
            {
                return;
            }

            if (ChessBoard.GetChessBoardObj().gGameStatus != ChessBoard.GameSatus.PLAYING)
            {
                return;
            }
                       
            ChessMan chessMan = chessBoard.currSelectChess;

            if (chessBoard.currSelectChess != null)
            {
                if (true == chessBoard.currSelectChess.MoveChess(des_row, des_colomn))
                {
                    chessBoard.currSelectChess.beSelected = false;
                    chessBoard.currSelectChess = null;
                    GameState.currentTokenLocate = Location.unknown;
                    GameState.currentAgreeHeQiNum = 0;
                    if (GameState.gameWin.HeQiBtn.IsEnabled)
                    {
                        GameState.gameWin.SetQiuHeButtonStatus(false);
                    }
                    if (GameState.gameWin.RenShuBtn.IsEnabled)
                    {
                        GameState.gameWin.SetRenShuButtonStatus(false);
                    }
                    ChessBoard.SetTargetGrid(true, ChessBoard.GetChessBoardObj().g_chess_board[chessMan.Row, chessMan.Column].chessGrid);
                }
            }
            else
            {
                //if (ChessBoard.selectElement != null)
                if (ChessBoard.IsSelected())
                {
                    //ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.selectElement);
                    ChessBoard.SetSelectGrid(false);
                }
                //if (ChessBoard.targetElement != null)
                if (ChessBoard.IsTargeted())
                {
                    //ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.targetElement);
                    ChessBoard.SetTargetGrid(false);
                }
            }
        }

        public static Point GetPointByGrid(byte row, byte column)
        {
            Point p = new Point();
            p.X = 2 + column * 50.4;
            p.Y = 2 + row * 50.4;
            return p;
        }

       public static ChessBoard GetChessBoardObj(Location currUserLocation = Location.unknown, Chess chessWin=null)
        {
            if (chessBoard == null)
            {
                if (currUserLocation != Location.unknown)
                {
                    chessWindow = chessWin;
                    //chessWindow.gridChessBoard.MouseLeftButtonDown += new MouseButtonEventHandler(gridChessBoard_MouseLeftButtonDown);
                    chessBoard = new ChessBoard(currUserLocation);
                }
            }

            return chessBoard;
        }

       public static void DestroryChessBoard()
       {
           chessBoard = null;
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
                    {
                        this.g_chess_board[i, j].status = BoardStatus.empty;
                        Grid chessGrid = new Grid();
                        Grid.SetRow(chessGrid, i);
                        Grid.SetColumn(chessGrid, j);
                        Grid.SetRowSpan(chessGrid, 1);
                        Grid.SetColumnSpan(chessGrid, 1);
                        chessGrid.MouseLeftButtonDown += new MouseButtonEventHandler(gridChessBoard_MouseLeftButtonDown);

                        chessGrid.Background = Brushes.Transparent;
                        chessWindow.gridChessBoard.Children.Add(chessGrid);
                        this.g_chess_board[i, j].chessGrid = chessGrid;
                    }
                }
            }

            this.currUserLocation = currUserLocation;
            this.InitailUsersInfo();
            gGameStatus = GameSatus.PLAYING;
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

                            //myBitmapImage.DecodePixelWidth = (int)chess.Width;
                            myBitmapImage.EndInit();
                            //set image source
                            chess.Source = myBitmapImage;
                        }
                    }
                }
            }
        }

        public void BackToOriginalOwner(User currOwner, User originalOwner)
        {
            if ((currOwner == null) || (originalOwner == null))
            {
                return;
            }

            for (byte i = 0; i < BOARD_ROW_NUM; ++i)
            {
                for (byte j = 0; j < BOARD_COLUMN_NUM; ++j)
                {
                    if ((this.g_chess_board[i, j].status == BoardStatus.occupied) && (this.g_chess_board[i, j].chess != null))
                    {
                        //Console.WriteLine("GetOwnUser:" + g_chess_board[i, j].chess.GetOwnUser() + "   GetOriginalOwnUser" + g_chess_board[i, j].chess.GetOriginalOwnUser());
                        //Console.WriteLine("Current chess name:" + g_chess_board[i, j].chess.GetChessName() + " imageUriSource:" + g_chess_board[i, j].chess.imageUriSource);
                        if ((this.g_chess_board[i, j].chess.GetOwnUser() == currOwner) &&
                            (this.g_chess_board[i, j].chess.GetOriginalOwnUser() == originalOwner))
                        {
                            ChessMan chess = this.g_chess_board[i, j].chess;
                            chess.SetOwnUser(originalOwner);
                            chess.InitialImageUriSource(chess.GetChessName());

                            // Create source
                            BitmapImage myBitmapImage = new BitmapImage();
                            myBitmapImage.BeginInit();
                            myBitmapImage.UriSource = new Uri(chess.imageUriSource, UriKind.Absolute);
                            Console.WriteLine("Current chess name:" + chess.GetChessName() + " imageUriSource:" + chess.imageUriSource);
                            //myBitmapImage.DecodePixelWidth = (int)chess.Width;
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

        public static void SetSelectGrid(bool selected, Grid grid=null)
        {
            if (grid != null)
                selectGrid = grid;

            if (selectGrid != null)
            {
                if (selected)
                {
                    BitmapImage myBitmapImage = new BitmapImage();
                    // BitmapImage.UriSource must be in a BeginInit/EndInit block
                    myBitmapImage.BeginInit();
                    myBitmapImage.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\selected.png", UriKind.Absolute);

                    myBitmapImage.EndInit();
                    selectGrid.Background = new ImageBrush(myBitmapImage);
                }else {
                    selectGrid.Background = Brushes.Transparent;
                }
            }
        }

        public static void SetTargetGrid(bool selected, Grid grid = null)
        {
            if (grid != null)
                targetGrid = grid;

            if (targetGrid != null)
            {
                if (selected)
                {
                    BitmapImage myBitmapImage = new BitmapImage();
                    // BitmapImage.UriSource must be in a BeginInit/EndInit block
                    myBitmapImage.BeginInit();
                    myBitmapImage.UriSource = new Uri(GameState.gWorkPath + @"\res\Images\selected.png", UriKind.Absolute);

                    myBitmapImage.EndInit();
                    targetGrid.Background = new ImageBrush(myBitmapImage);
                }
                else
                {
                    targetGrid.Background = Brushes.Transparent;
                }
            }
        }

        public static Grid GetSelectGrid()
        {
            return selectGrid;
        }

        public static Grid GetTargetGrid()
        {
            return targetGrid;
        }

        public static bool IsSelected()
        {
            bool ret = false;
            if (selectGrid != null)
            {
                if (selectGrid.Background == Brushes.Transparent)
                {
                    ret = false;
                }
                else
                {
                    ret = true ;
                }
            }

            return ret;
        }

        public static bool IsTargeted()
        {
            bool ret = false;
            if (targetGrid != null)
            {
                if (targetGrid.Background == Brushes.Transparent)
                {
                    ret = false;
                }
                else
                {
                    ret = true;
                }
            }

            return ret;
        }
    }
}

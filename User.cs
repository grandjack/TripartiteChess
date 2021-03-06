﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Controls;

namespace WpfApplication2
{
    public enum Location
    {
        left = 0,
        right = 1,
        bottom = 2,
        unknown = 3,
        middle = 4
    };

    abstract class User
    {
        public enum GameState { WON = 0, LOSE, PLAYING, UNKNOWN }

        protected Location location = Location.unknown;
        protected GameState state = GameState.UNKNOWN;
        public int score = 0;
        public string account = "";
        public string password = "";
        public string email = "";
        public string user_name = "Unknown";

        public ChessTimer timer = null;

        protected ChessBoard board = null;

        public void CreateChessImage(ChessMan chessMan, byte row, byte column)
        {
            //int wide = 45;
            board.g_chess_board[row, column].status = ChessBoard.BoardStatus.occupied;
            board.g_chess_board[row, column].chess = chessMan;
            board.g_chess_board[row, column].chess.Row = row;
            board.g_chess_board[row, column].chess.Column = column;

            //Image 的其他属性设置 as Source Background etc.
            Point p = ChessBoard.GetPointByGrid(row, column);
            board.g_chess_board[row, column].chess.XPoint = p.X;
            board.g_chess_board[row, column].chess.YPoint = p.Y;

            //chessMan.Width = wide;

            // Create source
            BitmapImage myBitmapImage = new BitmapImage();

            // BitmapImage.UriSource must be in a BeginInit/EndInit block
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(chessMan.imageUriSource, UriKind.Absolute);

            //myBitmapImage.DecodePixelWidth = wide;
            myBitmapImage.EndInit();
            //set image source
            chessMan.Source = myBitmapImage;
            chessMan.HorizontalAlignment = HorizontalAlignment.Center;
            chessMan.VerticalAlignment = VerticalAlignment.Center;
            //chessMan.Margin = new Thickness(5);//new Thickness(chessMan.XPoint, chessMan.YPoint, 0, 0);
            //chessMan.MouseMove += new MouseEventHandler(this.chess_MouseMove);
            chessMan.MouseEnter += new MouseEventHandler(chessMan_MouseEnter);
            chessMan.MouseLeave += new MouseEventHandler(chessMan_MouseLeave);
            chessMan.MouseLeftButtonDown += new MouseButtonEventHandler(this.chessMan_MouseLeftButtonDown);

            //ChessBoard.chessWindow.gridChessBoard.Children.Add(chessMan);
            board.g_chess_board[row, column].chessGrid.Children.Add(chessMan);
        }

        private void chessMan_MouseEnter(object sender, MouseEventArgs e)
        {
            ChessMan chessMan = sender as ChessMan;
            //只有该当前用户走棋的时候才需要显示手型鼠标
            //(ChessBoard.GetChessBoardObj().GetUserByUsrLocation((Location)WpfApplication2.GameState.locate).State != User.GameState.PLAYING)
            if ((board.currUserLocation == WpfApplication2.GameState.currentTokenLocate) &&
                (board.currentUser.State == User.GameState.PLAYING) &&
                (ChessBoard.GetChessBoardObj().gGameStatus == ChessBoard.GameSatus.PLAYING) &&
                ((chessMan.GetOwnUser() == board.currentUser) || (board.currSelectChess != null)))
            {
                chessMan.Cursor = Cursors.Hand;

                if (chessMan.GetChessType() == ChessMan.ChessSpec.CHESS_BING)
                {
                    DisplayBingTips.DisplayBingTip(chessMan.Row, chessMan.Column);
                }
            }

        }

        private void chessMan_MouseLeave(object sender, MouseEventArgs e)
        {
            ChessMan chessMan = sender as ChessMan;
            chessMan.Cursor = Cursors.Arrow;
            //ChessBoard.GetChessBoardObj().g_chess_board[chessMan.Row, chessMan.Column].chessGrid.Background = Brushes.Transparent;
            if (chessMan.GetChessType() == ChessMan.ChessSpec.CHESS_BING)
            {
                DisplayBingTips.DestroryBingTip();
            }
        }

        private void chessMan_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChessMan chessMan = sender as ChessMan;
            Console.WriteLine("Click Chess: " + chessMan.GetChessName() + " ,and it belong to " + chessMan.GetOwnUser().account);

            if ((!WpfApplication2.GameState.allUsersReady) || 
                (WpfApplication2.GameState.currentTokenLocate != (Location)WpfApplication2.GameState.locate) ||
                ((chessMan.GetOwnUser().location != (Location)WpfApplication2.GameState.locate) && ((board.currSelectChess == null) || !board.currSelectChess.beSelected)) )
            {
                e.Handled = true;
                return;
            }

            if ((ChessBoard.GetChessBoardObj().gGameStatus != ChessBoard.GameSatus.PLAYING) ||
                (ChessBoard.GetChessBoardObj().GetUserByUsrLocation((Location)WpfApplication2.GameState.locate).State != User.GameState.PLAYING))
            {
                return;
            }

            if (chessMan.GetChessType() == ChessMan.ChessSpec.CHESS_BING)
            {
                DisplayBingTips.DestroryBingTip();
            }

            if (board.currSelectChess == null)
            {
                //if (ChessBoard.selectElement != null)
                if (ChessBoard.IsSelected())
                {
                    //ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.selectElement);
                    ChessBoard.SetSelectGrid(false);
                }
                if (ChessBoard.IsTargeted())
                {
                    //ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.targetElement);
                    ChessBoard.SetTargetGrid(false);
                }
            }

            if ((board.currSelectChess != chessMan) && (chessMan.beSelected == false))
            {
                if (board.currSelectChess != null)
                {
                    //必须是己方棋子
                    if (board.currSelectChess.GetOwnUser() == chessMan.GetOwnUser())
                    {
                        //还原之前的选择
                        board.currSelectChess.beSelected = false;
                        board.currSelectChess.Opacity = 1;

                        //if (ChessBoard.selectElement != null)
                        if (ChessBoard.IsSelected())
                        {
                            //ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.selectElement);
                            ChessBoard.SetSelectGrid(false);
                        }
                    }
                    else//点击可能无效或者吃棋，吃棋操作将消息路由到gridboard
                    {
                        return;
                    }
                }

                chessMan.beSelected = true;
                chessMan.Opacity = 1;
                board.currSelectChess = chessMan;
                
                ChessBoard.SetSelectGrid(true, ChessBoard.GetChessBoardObj().g_chess_board[chessMan.Row, chessMan.Column].chessGrid);

                /*MediaPlayer player = new MediaPlayer();
                player.Open(new Uri(WpfApplication2.GameState.gWorkPath + @"\res\voice\select.wav", UriKind.Absolute));
                player.Play();*/
                MediaBackgroundThread.PlayMedia(MediaType.MEDIA_SELECT);

                //消息终止，不再路由到上层
                e.Handled = true;
            }
        }


        public Location GetUserLocation()
        {
            return this.location;
        }

        public int MinusScore(int dec)
        {
            this.score -= dec;
            return this.score;
        }

        public int PlusScore(int dec)
        {
            this.score += dec;
            return this.score;
        }

        public int GetCurrentScore()
        {
            return this.score;
        }

        public GameState State
        {
            get { return this.state; }
            set { this.state = value; }
        }

        public abstract bool InitialOwnChessBoard();
        public abstract bool CheckChessManScope(ChessMan.ChessSpec chessType, byte r_row, byte r_column, byte d_row, byte d_column);
        public abstract bool IfJiangJun();
    }

    class LeftUser : User
    {
        public LeftUser(ChessBoard board)
        {
            this.location = Location.left;
            this.board = board;
            this.state = GameState.PLAYING;
            this.InitialOwnChessBoard();
            this.timer = new ChessTimer(ChessBoard.total_time, ChessBoard.single_step_time, ChessBoard.chessWindow, this.location);
        }

        public override bool InitialOwnChessBoard()
        {
            this.CreateChessImage(new Troops(this), 0, 0);
            this.CreateChessImage(new Horse(this), 1, 0);
            this.CreateChessImage(new Elephant(this), 2, 0);
            this.CreateChessImage(new Guardsman(this), 3, 0);
            this.CreateChessImage(new Captain(this), 4, 0);
            this.CreateChessImage(new Guardsman(this), 5, 0);
            this.CreateChessImage(new Elephant(this), 6, 0);
            this.CreateChessImage(new Horse(this), 7, 0);
            this.CreateChessImage(new Troops(this), 8, 0);

            this.CreateChessImage(new Bullet(this), 1, 2);
            this.CreateChessImage(new Bullet(this), 7, 2);

            this.CreateChessImage(new Soldier(this), 0, 3);
            this.CreateChessImage(new Soldier(this), 2, 3);
            this.CreateChessImage(new Soldier(this), 4, 3);
            this.CreateChessImage(new Soldier(this), 6, 3);
            this.CreateChessImage(new Soldier(this), 8, 3);

            return true;
        }

        public override bool CheckChessManScope(ChessMan.ChessSpec chessType, byte r_row, byte r_column, byte d_row, byte d_column)
        {
            switch (chessType)
            {
                case ChessMan.ChessSpec.CHESS_SHUAI:
                case ChessMan.ChessSpec.CHESS_SHI:
                case ChessMan.ChessSpec.CHESS_XIANG:
                    return ChessBoard.ValidChessManScope(chessType, d_row, d_column);

                case ChessMan.ChessSpec.CHESS_BING:
                    if (ChessBoard.GetPointLocation(r_row, r_column) == Location.left)
                    {//只能向右走一步
                        if (!((d_column == r_column + 1) && (r_row == d_row)))
                        {
                            return false;
                        }
                    }
                    else if (ChessBoard.CheckIfPublicArea(r_row, r_column) || (ChessBoard.GetPointLocation(r_row, r_column) == Location.right))
                    //处于交战区和右方区域的规则,不能向左
                    //else if ((ChessBoard.GetPointLocation(r_row, r_column) == Location.middle) || (ChessBoard.GetPointLocation(r_row, r_column) == Location.right))
                    {
                        if (!((d_column - r_column <= 1) && (d_column >= r_column)))
                        {
                            return false;
                        }
                    }
                    else if (ChessBoard.GetPointLocation(r_row, r_column) == Location.bottom)
                    {//处于下方区域，不能向上
                        if (!((d_row - r_row <= 1) && (d_row >= r_row)))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;

                default:
                    break;
            }

            return true;
        }

        public override bool IfJiangJun()
        {
            if (ChessBoard.GetChessBoardObj().CheckIfJiangJun(this, ChessBoard.GetChessBoardObj().righttUser) ||
                ChessBoard.GetChessBoardObj().CheckIfJiangJun(this, ChessBoard.GetChessBoardObj().bottomUser))
                return true;

            return false;
        }
    }

    class RightUser : User
    {
        public RightUser(ChessBoard board)
        {
            this.location = Location.right;
            this.board = board;
            this.state = GameState.PLAYING;
            this.InitialOwnChessBoard();
            this.timer = new ChessTimer(ChessBoard.total_time, ChessBoard.single_step_time, ChessBoard.chessWindow, this.location);
        }

        public override bool InitialOwnChessBoard()
        {
            this.CreateChessImage(new Troops(this), 0, ChessBoard.BOARD_COLUMN_NUM - 1);
            this.CreateChessImage(new Horse(this), 1, ChessBoard.BOARD_COLUMN_NUM - 1);
            this.CreateChessImage(new Elephant(this), 2, ChessBoard.BOARD_COLUMN_NUM - 1);
            this.CreateChessImage(new Guardsman(this), 3, ChessBoard.BOARD_COLUMN_NUM - 1);
            this.CreateChessImage(new Captain(this), 4, ChessBoard.BOARD_COLUMN_NUM - 1);
            this.CreateChessImage(new Guardsman(this), 5, ChessBoard.BOARD_COLUMN_NUM - 1);
            this.CreateChessImage(new Elephant(this), 6, ChessBoard.BOARD_COLUMN_NUM - 1);
            this.CreateChessImage(new Horse(this), 7, ChessBoard.BOARD_COLUMN_NUM - 1);
            this.CreateChessImage(new Troops(this), 8, ChessBoard.BOARD_COLUMN_NUM - 1);

            this.CreateChessImage(new Bullet(this), 1, 16);
            this.CreateChessImage(new Bullet(this), 7, 16);

            this.CreateChessImage(new Soldier(this), 0, 15);
            this.CreateChessImage(new Soldier(this), 2, 15);
            this.CreateChessImage(new Soldier(this), 4, 15);
            this.CreateChessImage(new Soldier(this), 6, 15);
            this.CreateChessImage(new Soldier(this), 8, 15);
            return true;
        }

        public override bool CheckChessManScope(ChessMan.ChessSpec chessType, byte r_row, byte r_column, byte d_row, byte d_column)
        {
            switch (chessType)
            {
                case ChessMan.ChessSpec.CHESS_SHUAI:
                case ChessMan.ChessSpec.CHESS_SHI:
                case ChessMan.ChessSpec.CHESS_XIANG:
                    return ChessBoard.ValidChessManScope(chessType, d_row, d_column);

                case ChessMan.ChessSpec.CHESS_BING:
                    if (ChessBoard.GetPointLocation(r_row, r_column) == Location.right)
                    {//只能向左走一步
                        if (!((d_column == r_column - 1) && (r_row == d_row)))
                        {
                            return false;
                        }
                    }
                    //处于交战区和左方区域的规则,不能向右
                    else if (ChessBoard.CheckIfPublicArea(r_row, r_column) || ChessBoard.GetPointLocation(r_row, r_column) == Location.left)
                    //else if (ChessBoard.GetPointLocation(r_row, r_column) == Location.middle || ChessBoard.GetPointLocation(r_row, r_column) == Location.left)
                    {
                        if (!((r_column - d_column <= 1) && (d_column <= r_column)))
                        {
                            return false;
                        }
                    }
                    else if (ChessBoard.GetPointLocation(r_row, r_column) == Location.bottom)
                    {//处于下方区域
                        if (!((d_row - r_row <= 1) && (d_row >= r_row)))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;

                default:
                    break;
            }

            return true;
        }

        public override bool IfJiangJun()
        {
            if (ChessBoard.GetChessBoardObj().CheckIfJiangJun(this, ChessBoard.GetChessBoardObj().leftUser) ||
                ChessBoard.GetChessBoardObj().CheckIfJiangJun(this, ChessBoard.GetChessBoardObj().bottomUser))
                return true;

            return false;
        }
    }

    class BottomUser : User
    {
        public BottomUser(ChessBoard board)
        {
            this.location = Location.bottom;
            this.state = GameState.PLAYING;
            this.board = board;
            this.InitialOwnChessBoard();
            this.timer = new ChessTimer(ChessBoard.total_time, ChessBoard.single_step_time, ChessBoard.chessWindow, this.location);
        }

        public override bool InitialOwnChessBoard()
        {
            this.CreateChessImage(new Troops(this), ChessBoard.BOARD_ROW_NUM - 1, 5);
            this.CreateChessImage(new Horse(this), ChessBoard.BOARD_ROW_NUM - 1, 6);
            this.CreateChessImage(new Elephant(this), ChessBoard.BOARD_ROW_NUM - 1, 7);
            this.CreateChessImage(new Guardsman(this), ChessBoard.BOARD_ROW_NUM - 1, 8);
            this.CreateChessImage(new Captain(this), ChessBoard.BOARD_ROW_NUM - 1, 9);
            this.CreateChessImage(new Guardsman(this), ChessBoard.BOARD_ROW_NUM - 1, 10);
            this.CreateChessImage(new Elephant(this), ChessBoard.BOARD_ROW_NUM - 1, 11);
            this.CreateChessImage(new Horse(this), ChessBoard.BOARD_ROW_NUM - 1, 12);
            this.CreateChessImage(new Troops(this), ChessBoard.BOARD_ROW_NUM - 1, 13);

            this.CreateChessImage(new Bullet(this), ChessBoard.BOARD_ROW_NUM - 3, 6);
            this.CreateChessImage(new Bullet(this), ChessBoard.BOARD_ROW_NUM - 3, 12);


            this.CreateChessImage(new Soldier(this), ChessBoard.BOARD_ROW_NUM - 4, 5);
            this.CreateChessImage(new Soldier(this), ChessBoard.BOARD_ROW_NUM - 4, 7);
            this.CreateChessImage(new Soldier(this), ChessBoard.BOARD_ROW_NUM - 4, 9);
            this.CreateChessImage(new Soldier(this), ChessBoard.BOARD_ROW_NUM - 4, 11);
            this.CreateChessImage(new Soldier(this), ChessBoard.BOARD_ROW_NUM - 4, 13);
            return true;
        }

        public override bool CheckChessManScope(ChessMan.ChessSpec chessType, byte r_row, byte r_column, byte d_row, byte d_column)
        {
            switch (chessType)
            {
                case ChessMan.ChessSpec.CHESS_SHUAI:
                case ChessMan.ChessSpec.CHESS_SHI:
                case ChessMan.ChessSpec.CHESS_XIANG:
                    return ChessBoard.ValidChessManScope(chessType, d_row, d_column);

                case ChessMan.ChessSpec.CHESS_BING:
                    //if (r_row == 10 || r_row == 9)
                    if (ChessBoard.GetPointLocation(r_row, r_column) == Location.bottom)
                    {//只能向上走一步
                        if (!((d_row == r_row - 1) && (r_column == d_column)))
                        {
                            return false;
                        }
                    }
                    //处于交战区的规则,只能向上、左、右
                    //else if ((r_column > 4) && (r_column < 14) && (r_row >= 0) && (r_row < 9))
                    else if (ChessBoard.CheckIfPublicArea(r_row, r_column))
                    //else if (ChessBoard.GetPointLocation(r_row, r_column) == Location.middle)
                    {
                        if (!((r_row - d_row <= 1) && (r_row >= d_row)))
                        {
                            return false;
                        }
                    }
                    //else if (r_column <= 4)
                    else if (ChessBoard.GetPointLocation(r_row, r_column) == Location.left)
                    {//处于左方区域不能向右
                        if (!((r_column - d_column <= 1) && (r_column >= d_column)))
                        {
                            return false;
                        }
                    }
                    else if (ChessBoard.GetPointLocation(r_row, r_column) == Location.right)
                    {//处于右方区域不能向左
                        if (!((d_column - r_column <= 1) && (d_column >= r_column)))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;

                default:
                    break;
            }

            return true;
        }

        public override bool IfJiangJun()
        {
            if (ChessBoard.GetChessBoardObj().CheckIfJiangJun(this, ChessBoard.GetChessBoardObj().righttUser) ||
                ChessBoard.GetChessBoardObj().CheckIfJiangJun(this, ChessBoard.GetChessBoardObj().leftUser))
                return true;

            return false;
        }
    }


    class ChessTimer
    {
        public int total_time = 0;
        public int single_step_time = 0;
        public int curr_total_time = 0;
        public int curr_single_time = 0;
        public Location locate = Location.unknown;

        public Chess chessWin = null;
        System.Windows.Threading.DispatcherTimer dispatcherTimer = null;

        public ChessTimer(int total, int single, Window win, Location locate)
        {
            total_time = total;
            single_step_time = single;
            chessWin = (Chess)win;
            this.locate = locate;
            //  DispatcherTimer setup
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        }

        public void Start()
        {
            dispatcherTimer.Start();
        }
        public void Stop()
        {
            dispatcherTimer.Stop();
            curr_single_time = 0;
        }

        public void Reset()
        {
            total_time = 0;
            single_step_time = 0;
        }
        //  System.Windows.Threading.DispatcherTimer.Tick handler
        //
        //  Updates the current seconds display and calls
        //  InvalidateRequerySuggested on the CommandManager to force 
        //  the Command to raise the CanExecuteChanged event.
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            ++curr_total_time;
            ++curr_single_time;

            chessWin.DisplayTimer(curr_total_time, curr_single_time, locate);

            if (curr_total_time >= ChessBoard.total_time || curr_single_time >= ChessBoard.single_step_time)
            {
                chessWin.TimeoutHandle(locate);
                // Forcing the CommandManager to raise the RequerySuggested event
                CommandManager.InvalidateRequerySuggested();
                this.Stop();
                return;
            }
            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

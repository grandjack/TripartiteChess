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
    enum Location
    {
        unknown = 0,
        left = 0,
        right = 1,
        bottom = 2
        //middle = 3
    };

    abstract class User
    {
        public enum GameState { WON = 0, LOSE, PLAYING, UNKNOWN }

        protected Location location = Location.unknown;
        protected GameState state = GameState.UNKNOWN;
        protected int score = 100;
        protected string account = "";
        protected string password = "";
        protected string email = "";

        protected ChessBoard board = null;

        public void CreateChessImage(ChessMan chessMan, byte row, byte column)
        {
            int wide = 45;
            board.g_chess_board[row, column].status = ChessBoard.BoardStatus.occupied;
            board.g_chess_board[row, column].chess = chessMan;
            board.g_chess_board[row, column].chess.Row = row;
            board.g_chess_board[row, column].chess.Column = column;

            //Image 的其他属性设置 as Source Background etc.
            Point p = ChessBoard.GetPointByGrid(row, column);
            board.g_chess_board[row, column].chess.XPoint = p.X;
            board.g_chess_board[row, column].chess.YPoint = p.Y;

            chessMan.Width = wide;

            // Create source
            BitmapImage myBitmapImage = new BitmapImage();

            // BitmapImage.UriSource must be in a BeginInit/EndInit block
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(chessMan.imageUriSource, UriKind.Absolute);

            myBitmapImage.DecodePixelWidth = wide;
            myBitmapImage.EndInit();
            //set image source
            chessMan.Source = myBitmapImage;
            chessMan.HorizontalAlignment = HorizontalAlignment.Left;
            chessMan.VerticalAlignment = VerticalAlignment.Top;
            chessMan.Margin = new Thickness(chessMan.XPoint, chessMan.YPoint, 0, 0);
            chessMan.MouseMove += new MouseEventHandler(this.chess_MouseMove);
            chessMan.MouseLeftButtonDown += new MouseButtonEventHandler(this.chessMan_MouseLeftButtonDown);

            ChessBoard.chessWindow.gridChessBoard.Children.Add(chessMan);
        }

        private void chess_MouseMove(object sender, MouseEventArgs e)
        {
            ChessMan chessMan = sender as ChessMan;
            if (chessMan.GetOwnUser() == board.currentUser)
            {
                chessMan.Cursor = Cursors.Hand;
            }
        }

        private void chessMan_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChessMan chessMan = sender as ChessMan;

            if (board.currSelectChess == null)
            {
                ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.selectElement);
                ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.targetElement);
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

                        ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.selectElement);
                    }
                    else//点击可能无效或者吃棋
                    {
                        return;
                    }
                }

                chessMan.beSelected = true;
                chessMan.Opacity = 1;
                board.currSelectChess = chessMan;

                if (ChessBoard.selectElement != null)
                {
                    Image select_tag = ChessBoard.selectElement as Image;
                    select_tag.Margin = new Thickness(chessMan.Margin.Left, chessMan.Margin.Top - 1, chessMan.Margin.Right, chessMan.Margin.Bottom);

                    ChessBoard.chessWindow.gridChessBoard.Children.Add(ChessBoard.selectElement);
                }
                else
                {
                    Image select_tag = new Image();
                    select_tag.HorizontalAlignment = HorizontalAlignment.Left;
                    select_tag.VerticalAlignment = VerticalAlignment.Top;
                    select_tag.Margin = new Thickness(chessMan.Margin.Left, chessMan.Margin.Top-1, chessMan.Margin.Right, chessMan.Margin.Bottom);
                    select_tag.Visibility = Visibility.Visible;
                    select_tag.Width = chessMan.Width-1;
                    select_tag.Height = chessMan.Width-1;

                    // Create source
                    BitmapImage myBitmapImage = new BitmapImage();

                    // BitmapImage.UriSource must be in a BeginInit/EndInit block
                    myBitmapImage.BeginInit();
                    myBitmapImage.UriSource = new Uri(@"C:\Users\GBX386\Desktop\Visual C#\WpfApplication2\WpfApplication2\Images\selected.png", UriKind.Absolute);

                    myBitmapImage.DecodePixelWidth = (int)chessMan.Width;
                    myBitmapImage.EndInit();
                    //set image source
                    select_tag.Source = myBitmapImage;

                    select_tag.Opacity = 1;
                    select_tag.Cursor = Cursors.Hand;

                    ChessBoard.selectElement = select_tag;
                    ChessBoard.chessWindow.gridChessBoard.Children.Add(ChessBoard.selectElement);
                }

                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri(@"D:\My Software\QQGame\CChess\res\select.wav", UriKind.Absolute));
                player.Play();

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
                    //处于交战区和右方区域的规则
                    //else if ((r_column >= 5) && (r_column < ChessBoard.BOARD_COLUMN_NUM) && (r_row >= 0) && (r_row <= 8))
                    {
                        if (!((d_column - r_column <= 1) && (d_column >= r_column)))
                        {
                            return false;
                        }
                    }
                    else
                    {//处于下方区域
                        if (!((d_row - r_row <= 1) && (d_row >= r_row)))
                        {
                            return false;
                        }
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
                    //处于交战区和左方区域的规则
                    //else if ((r_column >= 0) && (r_column < 14) && (r_row >= 0) && (r_row <= 8))
                    else if (ChessBoard.CheckIfPublicArea(r_row, r_column) || ChessBoard.GetPointLocation(r_row, r_column) == Location.left)
                    {
                        if (!((r_column - d_column <= 1) && (d_column <= r_column)))
                        {
                            return false;
                        }
                    }
                    else
                    {//处于下方区域
                        if (!((d_row - r_row <= 1) && (d_row >= r_row)))
                        {
                            return false;
                        }
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
                    {
                        if (!((r_row - d_row <= 1) && (r_row >= d_row)))
                        {
                            return false;
                        }
                    }
                    else if (r_column <= 4)
                    {//处于左方区域
                        if (!((r_column - d_column <= 1) && (r_column >= d_column)))
                        {
                            return false;
                        }
                    }
                    else
                    {//处于右方区域
                        if (!((d_column - r_column <= 1) && (d_column >= r_column)))
                        {
                            return false;
                        }
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
}
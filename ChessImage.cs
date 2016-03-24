using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media;

namespace WpfApplication2
{
    abstract class ChessMan : Image
    {
        public enum ChessSpec
        {
            CHESS_CHE = 0,
            CHESS_MA,
            CHESS_PAO,
            CHESS_XIANG,
            CHESS_SHI,
            CHESS_SHUAI,
            CHESS_BING,
            CHESS_NUM
        }
        public enum MoveEvent { VALID_MOVE_EVENT, JUST_MOVE_EVENT, MOVE_TO_EAT_EVENT }
        public string[] ChessName = new string[(int)ChessSpec.CHESS_NUM]
        {
            "ju",
            "ma",
            "pao",
            "xiang",
            "shi",
            "shuai",
            "zu"
        };

        //图像在ChessBoard控件中的坐标位置
        protected double xPoint = 0.0;
        protected double yPoint = 0.0;
        //象棋在棋盘中的逻辑坐标，14*19
        protected byte row = 0;
        protected byte column = 0;


        protected User ownerUsr = null;
        protected User originalOwnerUsr = null;
        public bool beSelected = false;

        public string imageUriSourcePre = GameState.gWorkPath + @"\res\Images\ChessMan";
        public string imageUriSource = null;

        //定义属性
        public double XPoint
        {
            get { return this.xPoint; }
            set { this.xPoint = value; }
        }

        public double YPoint
        {
            get { return this.yPoint; }
            set { this.yPoint = value; }
        }

        public byte Row
        {
            get { return this.row; }
            set { 
                this.row = (byte)value;
                //this.YPoint = 2 + this.row * 50.4;
            }
        }

        public byte Column
        {
            get { return this.column; }
            set { 
                this.column = (byte)value;
                //this.XPoint = 2 + this.column * 50.4;
            }
        }

        public User GetOwnUser()
        {
            return this.ownerUsr;
        }

        public User GetOriginalOwnUser()
        {
            return this.originalOwnerUsr;
        }

        public void SetOwnUser(User user)
        {
            this.ownerUsr = user;
        }

        protected string chessName = null;
        protected ChessSpec chessSpec;


        public string GetChessName()
        {
            return this.chessName;
        }

        public ChessSpec GetChessType()
        {
            return this.chessSpec;
        }

        protected bool MoveToAction(byte des_row, byte des_column)
        {
            byte cur_row = this.row;
            byte cur_column = this.column;

            if (this.CheckCaptainFaceToFace(des_row, des_column))
            {
                Console.WriteLine("Jiang Shuai face to face, forbidden.");
                return false;
            }

            ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status = ChessBoard.BoardStatus.occupied;
            ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess = ChessBoard.GetChessBoardObj().g_chess_board[cur_row, cur_column].chess;
            ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.Row = des_row;
            ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.Column = des_column;

            //将棋子在目标grid中显示
            if (ChessBoard.GetChessBoardObj().g_chess_board[cur_row, cur_column].chessGrid.Children.Count > 0)
            {
                ChessBoard.GetChessBoardObj().g_chess_board[cur_row, cur_column].chessGrid.Children.Clear();
            }
            ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chessGrid.Children.Clear();
            ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chessGrid.Children.Add(ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess);

            //重新计算图像坐标位置
            Point p = ChessBoard.GetPointByGrid(des_row, des_column);
            ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.XPoint = p.X;
            ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.YPoint = p.Y;
            //ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.Margin = new Thickness(p.X, p.Y, 0, 0);

            ChessBoard.GetChessBoardObj().g_chess_board[cur_row, cur_column].status = ChessBoard.BoardStatus.empty;
            ChessBoard.GetChessBoardObj().g_chess_board[cur_row, cur_column].chess = null;
            

            this.row = des_row;
            this.column = des_column;

            return true;
        }

        protected bool EatAction(byte des_row, byte des_column)
        {
            bool eatCaptain = false;
            if (this.CheckCaptainFaceToFace(des_row, des_column))
            {
                Console.WriteLine("Will Face to face, forbidden!");
                return false;
            }

            User originUser = ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.GetOwnUser();
            // check whether eat 'Shuai' or 'Jiang jun' secondly
            if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.GetChessType() == ChessSpec.CHESS_SHUAI)
            {
                eatCaptain = true;
            }

            //将被吃方先减分
            originUser.MinusScore(10);
            //ChessBoard.chessWindow.gridChessBoard.Children.Remove(ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess);
            MoveToAction(des_row, des_column);
            //吃方加分
            ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.GetOwnUser().PlusScore(10);
            User move_user = ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.GetOwnUser();

            //first check whether the Shuai face to face 
            if (true == eatCaptain)
            {
                Console.WriteLine("You ate Captain!!");

                //MediaPlayer player = new MediaPlayer();
                if (originUser.GetUserLocation() == ChessBoard.GetChessBoardObj().currUserLocation)
                {
                    //player.Open(new Uri(GameState.gWorkPath + @"\res\voice\gameover.wav", UriKind.Absolute));
                    MediaBackgroundThread.PlayMedia(MediaType.MEDIA_OVER);
                }
                else
                {
                    MediaBackgroundThread.PlayMedia(MediaType.MEDIA_WIN);
                    //player.Open(new Uri(GameState.gWorkPath + @"\res\voice\gamewin.wav", UriKind.Absolute));
                }
                //player.Play();

                originUser.State = User.GameState.LOSE;
                if (ChessBoard.GetChessBoardObj().GetCurrentActiveUsrNum() <= 1)
                {
                    Console.WriteLine("You are the last winner!! Congratulations!");
                    move_user.State = User.GameState.WON;
                    ChessBoard.GetChessBoardObj().gGameStatus = ChessBoard.GameSatus.END;
                    return true;
                }
                else
                {
                    ChessBoard.GetChessBoardObj().UpdateChessOwner(originUser, this.ownerUsr);
                    //吃掉一方棋子
                }
            }

            return true;
        }

        protected bool CheckDesPointValid(byte des_row, byte des_column)
        {
            if (ChessBoard.IsValidPoint(des_row, des_column) != true)
            {
                //Console.WriteLine("Invalid Point!");
                return false;
            }

            if (this.beSelected == false)
            {
                return false;
            }

            //目的坐标是己方棋子，则返回
            if ((ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.occupied) &&
                (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.GetOwnUser() == this.ownerUsr))
            {
                return false;
            }

            if (this.ownerUsr.CheckChessManScope(chessSpec, this.row, this.column, des_row, des_column) != true)
            {
                return false;
            }

            return true;
        }

        protected bool CheckCaptainFaceToFace(byte des_row, byte des_column)
        {
            //保存原来位置棋子，检测如果走后是否会导致将帅相对
            bool ret = false;
            ChessBoard.BoardStatus realStatus = ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column].status;
            ChessBoard.BoardStatus desStatus = ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status;

            if ((ChessBoard.GetChessBoardObj().leftUser.State == User.GameState.PLAYING) &&
                (ChessBoard.GetChessBoardObj().righttUser.State == User.GameState.PLAYING))
            {
                ChessMan left_chess = ChessBoard.GetChessBoardObj().GetSpecOneChessByType(ChessBoard.GetChessBoardObj().leftUser, ChessMan.ChessSpec.CHESS_SHUAI);
                ChessMan right_chess = ChessBoard.GetChessBoardObj().GetSpecOneChessByType(ChessBoard.GetChessBoardObj().righttUser, ChessMan.ChessSpec.CHESS_SHUAI);

                if (left_chess == null || right_chess == null)
                {
                    return false;
                }
                else
                {
                    byte baseLeftRow = 0, baseRightRow = 0;
                    baseLeftRow = left_chess.Row;
                    baseRightRow = right_chess.Row;

                    /*当前走棋为帅*/
                    if (this == left_chess)
                    {
                        baseLeftRow = des_row;
                    }
                    else if (this == right_chess)
                    {
                        baseRightRow = des_row;
                    }
                    else if ((left_chess != this) && (right_chess != this))
                    {
                        ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column].status = ChessBoard.BoardStatus.empty;
                        ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status = ChessBoard.BoardStatus.occupied;
                    }
                    else
                    {
                        return false;
                    }

                    if (baseLeftRow == baseRightRow)
                    {
                        byte y = (byte)(left_chess.Column + 1);
                        for (; y < right_chess.Column; ++y)
                        {
                            if (ChessBoard.GetChessBoardObj().g_chess_board[left_chess.Row, y].status == ChessBoard.BoardStatus.occupied)
                            {
                                break;
                            }
                        }

                        if (y == right_chess.Column)
                        {
                            ret = true;
                        }
                    }
                }
            }

            ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column].status = realStatus;
            ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status = desStatus;
            return ret;
        }

        public bool MoveChess(byte des_row, byte des_column, bool shouldbrocast = true)
        {
            MoveEvent ret = this.CheckMoveEvent(des_row, des_column);
            bool status = false;
            byte src_row = this.Row;
            byte src_column = this.Column;

            ChessSpec origType = this.GetChessType();
            if (ret == MoveEvent.MOVE_TO_EAT_EVENT)
            {
                ChessSpec tarType = ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.GetChessType();
                int tarLocate = (int)ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.ownerUsr.GetUserLocation();
                User tarUsr = ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess.GetOwnUser();
                ChessMan tarchess = ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].chess;
                status = this.EatAction(des_row, des_column);
                if (status)
                {
                    /*MediaPlayer player = new MediaPlayer ();
                    player.Open(new Uri(GameState.gWorkPath + @"\res\voice\eat.wav", UriKind.Absolute));
                    player.Play();
                    */
                    MediaBackgroundThread.PlayMedia(MediaType.MEDIA_EAT);

                    //记录走棋路线
                    ChessBoard.gHuiQiStack.ate_chess = true;
                    ChessBoard.gHuiQiStack.from_x = src_row;
                    ChessBoard.gHuiQiStack.from_y = src_column;
                    ChessBoard.gHuiQiStack.des_x = des_row;
                    ChessBoard.gHuiQiStack.des_y = des_column;
                    ChessBoard.gHuiQiStack.chess = tarchess;

                    if (shouldbrocast)
                    {
                        GamePlayingState state = new GamePlayingState();
                        if (tarUsr.State == User.GameState.LOSE)
                        {
                            state.MoveChessReq((int)origType, GameState.locate, src_row, src_column, des_row, des_column, true, (int)tarType, tarLocate);
                        }
                        else
                        {
                            state.MoveChessReq((int)origType, GameState.locate, src_row, src_column, des_row, des_column, false, (int)tarType, tarLocate);
                        }
                    }
                }
            }
            else if (ret == MoveEvent.JUST_MOVE_EVENT)
            {
                status = this.MoveToAction(des_row, des_column); 
                if (status)
                {
                    /*MediaPlayer player = new MediaPlayer();
                    player.Open(new Uri(GameState.gWorkPath + @"\res\voice\go.wav", UriKind.Absolute));
                    player.Play();*/
                    MediaBackgroundThread.PlayMedia(MediaType.MEDIA_GO);
                    
                    //记录走棋路线
                    ChessBoard.gHuiQiStack.ate_chess = false;
                    ChessBoard.gHuiQiStack.from_x = src_row;
                    ChessBoard.gHuiQiStack.from_y = src_column;
                    ChessBoard.gHuiQiStack.des_x = des_row;
                    ChessBoard.gHuiQiStack.des_y = des_column;
                    ChessBoard.gHuiQiStack.chess = null;

                    if (shouldbrocast)
                    {
                        GamePlayingState state = new GamePlayingState();
                        state.MoveChessReq((int)origType, GameState.locate, src_row, src_column, des_row, des_column, false);
                    }
                }
            }

            //at last check whether Jiangjun
            if (this.ownerUsr.IfJiangJun() && status)
            {
                //播放将军提示你好                
                /*MediaPlayer player = new MediaPlayer();
                player.Open(new Uri(GameState.gWorkPath + @"\res\voice\dead.wav", UriKind.Absolute));
                player.Play();*/
                MediaBackgroundThread.PlayMedia(MediaType.MEDIA_DEAD);
            }

            return status;
        }

        public void InitialImageUriSource(string chessName)
        {
            switch (this.ownerUsr.GetUserLocation())
            {
                case Location.left:
                    this.imageUriSource = imageUriSourcePre + @"\BlackChess\" + chessName + ".png"; break;
                case Location.right:
                    this.imageUriSource = imageUriSourcePre + @"\GreenChess\" + chessName + ".png"; break;
                case Location.bottom:
                    this.imageUriSource = imageUriSourcePre + @"\RedChess\" + chessName + ".png"; break;
                default:
                    break;
            }
        }

        abstract public MoveEvent CheckMoveEvent(byte des_row, byte des_column);
    }

    class Horse : ChessMan
    {
        public Horse(User ownerUsr)
        {
            this.chessSpec = ChessSpec.CHESS_MA;
            this.chessName = ChessName[(int)this.chessSpec];
            this.ownerUsr = ownerUsr;
            this.originalOwnerUsr = ownerUsr;
            this.beSelected = false;

            this.InitialImageUriSource(this.chessName);
        }

        public override MoveEvent CheckMoveEvent(byte row, byte column)
        {
            MoveEvent ret = MoveEvent.VALID_MOVE_EVENT;
            byte x = row;
            byte y = column;

            if (this.CheckDesPointValid(x, y) != true)
            {
                Console.WriteLine("Invalid Point!");
                return ret;
            }

            //目标位置在当前位置之上
            if (this.row > x)
            {
                //第二象限
                if (this.column > y)
                {
                    if ((this.column - y <= 2) && (this.row - x <= 2))
                    {
                        if ((x == this.row - 1) && (y == this.column - 2))
                        {
                            if ((ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column - 1].status == ChessBoard.BoardStatus.empty)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column - 2].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row - 1, this.column - 1].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row - 1, this.column].status != ChessBoard.BoardStatus.illegal))
                            {
                                if ((ChessBoard.GetChessBoardObj().g_chess_board[x, y].status == ChessBoard.BoardStatus.occupied) &&
                                    (ChessBoard.GetChessBoardObj().g_chess_board[x, y].chess != null) &&
                                    (ChessBoard.GetChessBoardObj().g_chess_board[x, y].chess.GetOwnUser() != this.ownerUsr))
                                {
                                    ret = MoveEvent.MOVE_TO_EAT_EVENT;
                                }
                                else
                                {
                                    ret = MoveEvent.JUST_MOVE_EVENT;
                                }
                            }
                            else
                            {
                                return ret;
                            }
                        }
                        else if ((x == this.row - 2) && (y == this.column - 1))
                        {

                            if ((ChessBoard.GetChessBoardObj().g_chess_board[this.row - 1, this.column].status == ChessBoard.BoardStatus.empty)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row - 2, this.column].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row - 1, this.column - 1].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column - 1].status != ChessBoard.BoardStatus.illegal))
                            {
                                //目的坐标是己方棋子，则返回
                                if ((ChessBoard.GetChessBoardObj().g_chess_board[x, y].status == ChessBoard.BoardStatus.occupied) &&
                                    (ChessBoard.GetChessBoardObj().g_chess_board[x, y].chess.GetOwnUser() != this.ownerUsr))
                                {
                                    ret = MoveEvent.MOVE_TO_EAT_EVENT;
                                }
                                else
                                {
                                    ret = MoveEvent.JUST_MOVE_EVENT;
                                }
                            }
                            else
                            {
                                return ret;
                            }
                        }
                        else
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        return ret;
                    }
                }
                //第一象限
                else if (this.column < y)
                {
                    if ((y - this.column <= 2) && (this.row - x <= 2))
                    {
                        if ((x == this.row - 1) && (this.column == y - 2))
                        {
                            if ((ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column + 1].status == ChessBoard.BoardStatus.empty)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column + 2].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row - 1, this.column + 1].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row - 1, this.column].status != ChessBoard.BoardStatus.illegal))
                            {
                                //目的坐标是敌方棋子，则吃掉
                                if ((ChessBoard.GetChessBoardObj().g_chess_board[x, y].status == ChessBoard.BoardStatus.occupied) &&
                                    (ChessBoard.GetChessBoardObj().g_chess_board[x, y].chess.GetOwnUser() != this.ownerUsr))
                                {
                                    ret = MoveEvent.MOVE_TO_EAT_EVENT;
                                }
                                else
                                {
                                    ret = MoveEvent.JUST_MOVE_EVENT;
                                }
                            }
                            else
                            {
                                return ret;
                            }
                        }
                        else if ((x == this.row - 2) && (this.column == y - 1))
                        {

                            if ((ChessBoard.GetChessBoardObj().g_chess_board[this.row - 1, this.column].status == ChessBoard.BoardStatus.empty)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row - 2, this.column].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row - 1, this.column + 1].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column + 1].status != ChessBoard.BoardStatus.illegal))
                            {
                                if ((ChessBoard.GetChessBoardObj().g_chess_board[x, y].status == ChessBoard.BoardStatus.occupied) &&
                                    (ChessBoard.GetChessBoardObj().g_chess_board[x, y].chess.GetOwnUser() != this.ownerUsr))
                                {
                                    ret = MoveEvent.MOVE_TO_EAT_EVENT;
                                }
                                else
                                {
                                    ret = MoveEvent.JUST_MOVE_EVENT;
                                }
                            }
                            else
                            {
                                return ret;
                            }
                        }
                        else
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        return ret;
                    }
                }
                else
                {
                    return ret;
                }
            }
            //目标位置在当前位置之下
            else if (this.row < x)
            {
                //第三象限
                if (this.column > y)
                {
                    if ((this.column - y <= 2) && (x - this.row <= 2))
                    {
                        if ((this.row == x - 1) && (y == this.column - 2))
                        {
                            if ((ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column - 1].status == ChessBoard.BoardStatus.empty)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column - 2].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row + 1, this.column - 1].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row + 1, this.column].status != ChessBoard.BoardStatus.illegal))
                            {
                                //目的坐标是己方棋子，则返回
                                if ((ChessBoard.GetChessBoardObj().g_chess_board[x, y].status == ChessBoard.BoardStatus.occupied) &&
                                    (ChessBoard.GetChessBoardObj().g_chess_board[x, y].chess.GetOwnUser() != this.ownerUsr))
                                {
                                    ret = MoveEvent.MOVE_TO_EAT_EVENT;
                                }
                                else
                                {
                                    ret = MoveEvent.JUST_MOVE_EVENT;
                                }
                            }
                            else
                            {
                                return ret;
                            }
                        }
                        else if ((this.row == x - 2) && (y == this.column - 1))
                        {

                            if ((ChessBoard.GetChessBoardObj().g_chess_board[this.row + 1, this.column].status == ChessBoard.BoardStatus.empty)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row + 2, this.column].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row + 1, this.column - 1].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column - 1].status != ChessBoard.BoardStatus.illegal))
                            {
                                if ((ChessBoard.GetChessBoardObj().g_chess_board[x, y].status == ChessBoard.BoardStatus.occupied) &&
                                    (ChessBoard.GetChessBoardObj().g_chess_board[x, y].chess.GetOwnUser() != this.ownerUsr))
                                {
                                    ret = MoveEvent.MOVE_TO_EAT_EVENT;
                                }
                                else
                                {
                                    ret = MoveEvent.JUST_MOVE_EVENT;
                                }
                            }
                            else
                            {
                                return ret;
                            }
                        }
                        else
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        return ret;
                    }
                }
                //第四象限
                else if (this.column < y)
                {
                    if ((y - this.column <= 2) && (x - this.row <= 2))
                    {
                        if ((this.row == x - 1) && (this.column == y - 2))
                        {
                            if ((ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column + 1].status == ChessBoard.BoardStatus.empty)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column + 2].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row + 1, this.column + 1].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row + 1, this.column].status != ChessBoard.BoardStatus.illegal))
                            {
                                if ((ChessBoard.GetChessBoardObj().g_chess_board[x, y].status == ChessBoard.BoardStatus.occupied) &&
                                    (ChessBoard.GetChessBoardObj().g_chess_board[x, y].chess.GetOwnUser() != this.ownerUsr))
                                {
                                    ret = MoveEvent.MOVE_TO_EAT_EVENT;
                                }
                                else
                                {
                                    ret = MoveEvent.JUST_MOVE_EVENT;
                                }
                            }
                            else
                            {
                                return ret;
                            }
                        }
                        else if ((this.row == x - 2) && (this.column == y - 1))
                        {

                            if ((ChessBoard.GetChessBoardObj().g_chess_board[this.row + 1, this.column].status == ChessBoard.BoardStatus.empty)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row + 2, this.column].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row + 1, this.column + 1].status != ChessBoard.BoardStatus.illegal)
                                && (ChessBoard.GetChessBoardObj().g_chess_board[this.row, this.column + 1].status != ChessBoard.BoardStatus.illegal))
                            {
                                if ((ChessBoard.GetChessBoardObj().g_chess_board[x, y].status == ChessBoard.BoardStatus.occupied) &&
                                    (ChessBoard.GetChessBoardObj().g_chess_board[x, y].chess.GetOwnUser() != this.ownerUsr))
                                {
                                    ret = MoveEvent.MOVE_TO_EAT_EVENT;
                                }
                                else
                                {
                                    ret = MoveEvent.JUST_MOVE_EVENT;
                                }
                            }
                            else
                            {
                                return ret;
                            }
                        }
                        else
                        {
                            return ret;
                        }
                    }
                    else
                    {
                        return ret;
                    }
                }
                else
                {
                    return ret;
                }
            }
            else
            {
                return ret;
            }


            return ret;
        }
    }

    class Elephant : ChessMan
    {
        public Elephant(User ownerUsr)
        {
            this.chessSpec = ChessSpec.CHESS_XIANG;
            this.chessName = ChessName[(int)this.chessSpec];
            this.ownerUsr = ownerUsr;
            this.originalOwnerUsr = ownerUsr;
            this.beSelected = false;

            this.InitialImageUriSource(this.chessName);
        }

        public override MoveEvent CheckMoveEvent(byte des_row, byte des_column)
        {
            MoveEvent ret = MoveEvent.VALID_MOVE_EVENT;

            if (this.CheckDesPointValid(des_row, des_column) != true)
            {
                Console.WriteLine("Invalid Point!");
                return ret;
            }

            if ((System.Math.Abs((int)this.row - (int)des_row) == 2) &&
                (System.Math.Abs((int)this.column - (int)des_column) == 2))
            {
                int mid_x = (this.row + des_row) / 2;
                int mid_y = (this.column + des_column) / 2;

                if (ChessBoard.GetChessBoardObj().g_chess_board[mid_x, mid_y].status == ChessBoard.BoardStatus.empty)
                {
                    if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                        ret = MoveEvent.JUST_MOVE_EVENT;
                    else
                        ret = MoveEvent.MOVE_TO_EAT_EVENT;
                }
                else
                {
                    return ret;
                }
            }
            else
            {
                return ret;
            }

            return ret;
        }
    }

    //炮弹
    class Bullet : ChessMan
    {
        public Bullet(User ownerUsr)
        {
            this.chessSpec = ChessSpec.CHESS_PAO;
            this.chessName = ChessName[(int)this.chessSpec];
            this.ownerUsr = ownerUsr;
            this.originalOwnerUsr = ownerUsr;
            this.beSelected = false; 
            this.InitialImageUriSource(this.chessName);
        }

        public override MoveEvent CheckMoveEvent(byte des_row, byte des_column)
        {
            MoveEvent ret = MoveEvent.VALID_MOVE_EVENT;
            if (this.CheckDesPointValid(des_row, des_column) != true)
            {
                Console.WriteLine("Invalid Point!");
                return ret;
            }

            //在同行
            if (des_row == this.row)
            {
                if (des_column > this.column)
                {
                    byte hill_num = 0;
                    for (int i = this.column + 1; i < des_column; ++i)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[this.row, i].status != ChessBoard.BoardStatus.empty)
                        {
                            ++hill_num;
                        }
                    }
                    if (hill_num == 0)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                            ret = MoveEvent.JUST_MOVE_EVENT;
                        else
                            return ret;
                    }
                    else if (hill_num == 1)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status != ChessBoard.BoardStatus.empty)
                            ret = MoveEvent.MOVE_TO_EAT_EVENT;
                        else
                            return ret;
                    }
                    else
                    {
                        return ret;
                    }
                }
                else if (des_column < this.column)
                {
                    byte hill_num = 0;
                    for (int i = this.column - 1; i > des_column; --i)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[this.row, i].status != ChessBoard.BoardStatus.empty)
                        {
                            ++hill_num;
                        }
                    }
                    if (hill_num == 0)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                            ret = MoveEvent.JUST_MOVE_EVENT;
                        else
                            return ret;
                    }
                    else if (hill_num == 1)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status != ChessBoard.BoardStatus.empty)
                            ret = MoveEvent.MOVE_TO_EAT_EVENT;
                        else
                            return ret;
                    }
                    else
                    {
                        return ret;
                    }
                }
                else
                {
                    return ret;
                }
            }
            //在同列
            else if (des_column == this.column)
            {
                if (this.row < des_row)
                {
                    byte hill_num = 0;
                    for (int i = this.row + 1; i < des_row; ++i)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[i, this.column].status != ChessBoard.BoardStatus.empty)
                        {
                            ++hill_num;
                        }
                    }
                    if (hill_num == 0)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                            ret = MoveEvent.JUST_MOVE_EVENT;
                        else
                            return ret;
                    }
                    else if (hill_num == 1)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status != ChessBoard.BoardStatus.empty)
                            ret = MoveEvent.MOVE_TO_EAT_EVENT;
                        else
                            return ret;
                    }
                    else
                    {
                        return ret;
                    }
                }
                else if (this.row > des_row)
                {
                    byte hill_num = 0;
                    for (int i = this.row - 1; i > des_row; --i)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[i, this.column].status != ChessBoard.BoardStatus.empty)
                        {
                            ++hill_num;
                        }
                    }
                    if (hill_num == 0)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                            ret = MoveEvent.JUST_MOVE_EVENT;
                        else
                            return ret;
                    }
                    else if (hill_num == 1)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status != ChessBoard.BoardStatus.empty)
                            ret = MoveEvent.MOVE_TO_EAT_EVENT;
                        else
                            return ret;
                    }
                    else
                    {
                        return ret;
                    }
                }
                else
                {
                    return ret;
                }
            }
            else
            {
                return ret;
            }

            return ret;
        }

    }

    //士兵
    class Soldier : ChessMan
    {
        public Soldier(User ownerUsr)
        {
            this.chessSpec = ChessSpec.CHESS_BING;
            this.chessName = ChessName[(int)this.chessSpec];
            this.ownerUsr = ownerUsr;
            this.originalOwnerUsr = ownerUsr;
            this.beSelected = false;
            this.InitialImageUriSource(this.chessName);
        }

        public override MoveEvent CheckMoveEvent(byte des_row, byte des_column)
        {
            MoveEvent ret = MoveEvent.VALID_MOVE_EVENT;

            if (this.CheckDesPointValid(des_row, des_column) != true)
            {
                Console.WriteLine("Invalid Point!");
                return ret;
            }

            if (des_row == this.row)
            {
                if (System.Math.Abs((int)this.column - (int)des_column) == 1)
                {
                    if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                        ret = MoveEvent.JUST_MOVE_EVENT;
                    else
                        ret = MoveEvent.MOVE_TO_EAT_EVENT;
                }
                else
                {
                    return ret;
                }
            }
            else if (des_column == this.column)
            {
                if (System.Math.Abs((int)this.row - (int)des_row) == 1)
                {
                    if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                        ret = MoveEvent.JUST_MOVE_EVENT;
                    else
                        ret = MoveEvent.MOVE_TO_EAT_EVENT;
                }
                else
                {
                    return ret;
                }
            }
            else
            {
                return ret;
            }

            return ret;
        }
    }


    //Che 军
    class Troops : ChessMan
    {
        public Troops(User ownerUsr)
        {
            this.chessSpec = ChessSpec.CHESS_CHE;
            this.chessName = ChessName[(int)this.chessSpec];
            this.ownerUsr = ownerUsr;
            this.originalOwnerUsr = ownerUsr;
            this.beSelected = false;
            this.InitialImageUriSource(this.chessName);
        }

        public override MoveEvent CheckMoveEvent(byte des_row, byte des_column)
        {
            MoveEvent ret = MoveEvent.VALID_MOVE_EVENT;

            if (this.CheckDesPointValid(des_row, des_column) != true)
            {
                Console.WriteLine("Invalid Point!");
                return ret;
            }

            //在同行
            if (des_row == this.row)
            {
                if (des_column > this.column)
                {
                    bool des_valid = true;
                    for (int i = this.column + 1; i < des_column; ++i)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[this.row, i].status != ChessBoard.BoardStatus.empty)
                        {
                            des_valid = false;
                            break;
                        }
                    }

                    if (des_valid)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                            ret = MoveEvent.JUST_MOVE_EVENT;
                        else
                            ret = MoveEvent.MOVE_TO_EAT_EVENT;
                    }
                    else
                    {
                        return ret;
                    }
                }
                else if (des_column < this.column)
                {
                    bool des_valid = true;
                    for (int i = this.column - 1; i > des_column; --i)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[this.row, i].status != ChessBoard.BoardStatus.empty)
                        {
                            des_valid = false;
                            break;
                        }
                    }

                    if (des_valid)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                            ret = MoveEvent.JUST_MOVE_EVENT;
                        else
                            ret = MoveEvent.MOVE_TO_EAT_EVENT;
                    }
                    else
                    {
                        return ret;
                    }
                }
                else
                {
                    return ret;
                }
            }
            //在同列
            else if (des_column == this.column)
            {
                if (this.row < des_row)
                {
                    bool des_valid = true;
                    for (int i = this.row + 1; i < des_row; ++i)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[i, this.column].status != ChessBoard.BoardStatus.empty)
                        {
                            des_valid = false;
                            break;
                        }
                    }

                    if (des_valid)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                            ret = MoveEvent.JUST_MOVE_EVENT;
                        else
                            ret = MoveEvent.MOVE_TO_EAT_EVENT;
                    }
                    else
                    {
                        return ret;
                    }
                }
                else if (this.row > des_row)
                {
                    bool des_valid = true;
                    for (int i = this.row - 1; i > des_row; --i)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[i, this.column].status != ChessBoard.BoardStatus.empty)
                        {
                            des_valid = false;
                            break;
                        }
                    }

                    if (des_valid)
                    {
                        if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                            ret = MoveEvent.JUST_MOVE_EVENT;
                        else
                            ret = MoveEvent.MOVE_TO_EAT_EVENT;
                    }
                    else
                    {
                        return ret;
                    }
                }
                else
                {
                    return ret;
                }
            }
            else
            {
                return ret;
            }

            return ret;
        }
    }

    //帅 将
    class Captain : ChessMan
    {
        public Captain(User ownerUsr)
        {
            this.chessSpec = ChessSpec.CHESS_SHUAI;
            this.chessName = ChessName[(int)this.chessSpec];
            this.ownerUsr = ownerUsr;
            this.originalOwnerUsr = ownerUsr;
            this.beSelected = false;
            this.InitialImageUriSource(this.chessName);
        }

        public override MoveEvent CheckMoveEvent(byte des_row, byte des_column)
        {
            MoveEvent ret = MoveEvent.VALID_MOVE_EVENT;

            if (this.CheckDesPointValid(des_row, des_column) != true)
            {
                Console.WriteLine("Invalid Point!");
                return ret;
            }

            if (des_row == this.row)
            {
                if (System.Math.Abs((int)this.column - (int)des_column) == 1)
                {
                    if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                        ret = MoveEvent.JUST_MOVE_EVENT;
                    else
                        ret = MoveEvent.MOVE_TO_EAT_EVENT;
                }
                else
                {
                    return ret;
                }
            }
            else if (des_column == this.column)
            {
                if (System.Math.Abs((int)this.row - (int)des_row) == 1)
                {
                    if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                        ret = MoveEvent.JUST_MOVE_EVENT;
                    else
                        ret = MoveEvent.MOVE_TO_EAT_EVENT;
                }
                else
                {
                    return ret;
                }
            }
            else
            {
                return ret;
            }

            return ret;
        }
    }

    //侍卫
    class Guardsman : ChessMan
    {
        public Guardsman(User ownerUsr)
        {
            this.chessSpec = ChessSpec.CHESS_SHI;
            this.chessName = ChessName[(int)this.chessSpec];
            this.ownerUsr = ownerUsr;
            this.originalOwnerUsr = ownerUsr;
            this.beSelected = false;
            this.InitialImageUriSource(this.chessName);
        }

        public override MoveEvent CheckMoveEvent(byte des_row, byte des_column)
        {
            MoveEvent ret = MoveEvent.VALID_MOVE_EVENT;


            if (this.CheckDesPointValid(des_row, des_column) != true)
            {
                Console.WriteLine("Invalid Point!");
                return ret;
            }

            if ((System.Math.Abs((int)this.row - (int)des_row) == 1) &&
                (System.Math.Abs((int)this.column - (int)des_column) == 1))
            {
                if (ChessBoard.GetChessBoardObj().g_chess_board[des_row, des_column].status == ChessBoard.BoardStatus.empty)
                    ret = MoveEvent.JUST_MOVE_EVENT;
                else
                    ret = MoveEvent.MOVE_TO_EAT_EVENT;
            }
            else
            {
                return ret;
            }

            return ret;
        }
    }
}

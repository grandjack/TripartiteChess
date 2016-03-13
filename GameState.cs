using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.ProtocolBuffers;
using MessageStruct;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace WpfApplication2
{
    public enum MessageType
    {
        MSG_UNKNOWN = 0,
        MSG_ECHO,
        MSG_LOGIN,
        MSG_LOGIN_REPLY,
        MSG_LOGOUT,
        MSG_LOGOUT_REPLY,
        MSG_REGISTER,
        MSG_REGISTER_REPLY,
        MSG_CHESS_BOARD_USER,
        MSG_CHESS_BOARD_REQ,
        MSG_CHESS_BOARD,
        MSG_HALL_INFO_REQ,
        MSG_HALL_INFO,
        MSG_GAME_HALL_SUMARY_REQ,
        MSG_GAME_HALL_SUMARY,
        MSG_REQUEST_PLAY,
        MSG_REQUEST_PLAY_REPLY,
        MSG_MOVE_CHESS,
        MSG_ANNOUNCE_MOVE,
        MSG_USER_MSG,
        MSG_SYSTEM_MSG,
        MSG_RECONCILED_REQ,
        MSG_RECONCILED_RESP,
        MSG_GIVE_UP,
        MSG_UNDO_REQ,
        MSG_UNDO_REPS,
        MSG_GAME_READY_REQ,
        MSG_GAME_STATUS,
        MSG_FIND_PASSWORD,
        MSG_UPDATE_USER_INFO,
        MSG_AD_IMAGE_INFO,
        MSG_AD_IMAGE_REQ,
        MSG_AD_IMAGE_CONTENT,
        MSG_TYPE_MAX
    }

    public enum ImageID
    {
        IMAGE_ID_AD_HALL_TOP_LEFT = 0,
        IMAGE_ID_AD_HALL_TOP_RIGHT,
        IMAGE_ID_AD_HALL_BOTTOM_LEFT,
        IMAGE_ID_AD_HALL_BOTTOM_RIGHT,
        IMAGE_ID_AD_BOARD_TOP_LEFT,
        IMAGE_ID_AD_BOARD_TOP_RIGHT,
        IMAGE_ID_AD_BOARD_BOTTOM_LEFT,
        IMAGE_ID_AD_BOARD_BOTTOM_RIGHT,
        IMAGE_ID_AD_BOARD_MIDDLE,
        IMAGE_ID_LOGIN_BACKGROUND,
        IMAGE_ID_MAX
    }

    abstract class GameState
    {
        static public int gameHallID = 0;
        static public int chessBoardID = 0;
        static public int locate = 0;
        static public Location currentTokenLocate = Location.unknown;
        static public bool allUsersReady = false;
        
        //Current user info
        static public string currentUserName = null;
        static public string currentUserPassword = null;
        static public string currentUserAccount = null;
        static public string currentUserEmail = null;
        static public string currentUserPhoneNo = null;
        static public int currentUserScore = 0;
        static public byte[] currentUserHeadImage = null;

        static public MainWindow logginWin;
        static public Window1 gameHallWin;
        static public Chess gameWin;
        static public Window currentWin;
        static public GameHallSumary sumary = null;
        static public HallInfo hallInfo = null;
        static public ChessBoardInfo chessBoard = null;

        static public void SetCurrentWin(Window win)
        {
            currentWin = win;
        }
        static public void SetGameHallWin(Window win)
        {
            gameHallWin = (Window1)win;
        }
        static public void SetGameWin(Window win)
        {
            gameWin = (Chess)win;
        }

        abstract public bool MessageHandle(MessageType msg_type, byte[] msg);

        protected bool HandleEcho(byte[] msg)
        {
            try
            {
                Echo echo = Echo.ParseFrom(msg);
                Console.WriteLine("echo.time_stamp [" + echo.TimeStamp + "]");
            }
            catch (InvalidProtocolBufferException e)
            {
                Console.WriteLine("ParseFrom failed as {0}", e.Message);
            }
            return true;
        }

        protected bool GameHallSumayHandle(byte[] msg)
        {
            Console.WriteLine("Get MSG_GAME_HALL_SUMARY msg.");

            sumary = GameHallSumary.ParseFrom(msg);
            Console.WriteLine("Account " + sumary.Account);
            Console.WriteLine("UserName " + sumary.Username);
            Console.WriteLine("Score " + sumary.Score);
            Console.WriteLine("HallNum " + sumary.HallNum);
            Console.WriteLine("HeadPicture " + sumary.HeadPicture);
            Console.WriteLine("AdPicture1 " + sumary.AdPicture1);

            for (uint i = 1; i <= sumary.HallNum; ++i)
            {
                HallInfo hallinfo = sumary.GetHallInfo((int)(i - 1));

                Console.WriteLine("GameHallID " + hallinfo.GameHallId);
                Console.WriteLine("TotalPeople " + hallinfo.TotalPeople);
                Console.WriteLine("CurrPeople " + hallinfo.CurrPeople);
                Console.WriteLine("TotalChessBoard " + hallinfo.TotalChessboard);
            }

           currentWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                                {
                                    //if ((((Window1)currentWin).tab_game_hall.IsSelected == true) && (((Window1)currentWin).tab_game_hall.Visibility == Visibility.Visible))
                                    {
                                        gameHallWin.HallListBoxLoaded(null, null);
                                    }
                                }
                            );

           
            return true;
        }

        protected bool GameHallReplyHandle(byte[] msg)
        {
            try
            {
                hallInfo = HallInfo.ParseFrom(msg);
                Console.WriteLine("GamehallID: " + hallInfo.GameHallId);
                Console.WriteLine("TotalPeople: " + hallInfo.TotalPeople);
                Console.WriteLine("CurrPeople: " + hallInfo.CurrPeople);
                Console.WriteLine("TotalChessboard: " + hallInfo.TotalChessboard);

           
                for (uint i = 1; i <= hallInfo.TotalChessboard; ++i)
                {
                    ChessBoardInfo chessBoard = hallInfo.GetChessBoard((int)(i - 1));
                    Console.WriteLine("ChessBoardID: " + chessBoard.Id);
                    Console.WriteLine("PeopleNum: " + chessBoard.PeopleNum);
                    Console.WriteLine("LeftUser.ChessBoardEmpty: " + chessBoard.LeftUser.ChessBoardEmpty);
                    if (!chessBoard.LeftUser.ChessBoardEmpty)
                    {
                        Console.WriteLine("LeftUser.Account: " + chessBoard.LeftUser.Account);
                        Console.WriteLine("LeftUser.UserName: " + chessBoard.LeftUser.UserName);
                        Console.WriteLine("LeftUser.Score: " + chessBoard.LeftUser.Score);
                        Console.WriteLine("LeftUser.Status: " + chessBoard.LeftUser.Status);
                    }

                    Console.WriteLine("RightUser.ChessBoardEmpty: " + chessBoard.RightUser.ChessBoardEmpty);
                    if (!chessBoard.RightUser.ChessBoardEmpty)
                    {
                        Console.WriteLine("RightUser.Account: " + chessBoard.RightUser.Account);
                        Console.WriteLine("RightUser.UserName: " + chessBoard.RightUser.UserName);
                        Console.WriteLine("RightUser.Score: " + chessBoard.RightUser.Score);
                        Console.WriteLine("RightUser.Status: " + chessBoard.RightUser.Status);
                    }

                    Console.WriteLine("BottomUser.ChessBoardEmpty: " + chessBoard.BottomUser.ChessBoardEmpty);
                    if (!chessBoard.BottomUser.ChessBoardEmpty)
                    {
                        Console.WriteLine("BottomUser.Account: " + chessBoard.BottomUser.Account);
                        Console.WriteLine("BottomUser.UserName: " + chessBoard.BottomUser.UserName);
                        Console.WriteLine("BottomUser.Score: " + chessBoard.BottomUser.Score);
                        Console.WriteLine("BottomUser.Status: " + chessBoard.BottomUser.Status);
                    }
                }

               gameHallWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                    {
                        gameHallWin.game_hall_loaded(null, null);
                    }
                );
            }
            catch (Exception e)
            {
                Console.WriteLine("Parse failed for " + e.Message);
            }

            return true;
        }
    }

    class LoginState : GameState
    {
        public override bool MessageHandle(MessageType msg_type, byte[] msg)
        {
            switch (msg_type)
            {
                case MessageType.MSG_REGISTER_REPLY:
                    //反序列化
                    try
                    {
                        ReplyStatus reply = ReplyStatus.ParseFrom(msg);
                        Console.WriteLine("Register reply.Status " + reply.Status);
                        if (reply.Status == 1)
                        {
                            currentWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                                {
                                    ((MainWindow)currentWin).CancelBtn(null, null);
                                }
                            );
                        }
                        else
                        {
                            currentWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                                {
                                    MessageBox.Show("注册失败，该账号可能已经被注册");
                                }
                            );
                        }
                    }
                    catch (InvalidProtocolBufferException e)
                    {
                        Console.WriteLine("WriteLine failed as {0}", e.Message);
                    }
                    break;

                case MessageType.MSG_LOGOUT_REPLY:
                    break;

                case MessageType.MSG_LOGIN_REPLY:
                    //反序列化
                    try
                    {
                        ReplyStatus reply = ReplyStatus.ParseFrom(msg);
                        Console.WriteLine("reply.Status " + reply.Status);
                        if (reply.Status == 1)
                        {
                            if (reply.HasUser)
                            {
                                GameState.currentUserName = reply.User.UserName;
                                GameState.currentUserAccount = reply.User.Account;
                                GameState.currentUserEmail = reply.User.ExEmail;
                                GameState.currentUserPhoneNo = reply.User.Phone;
                                GameState.currentUserScore = reply.User.Score;
                                GameState.currentUserHeadImage = new byte[reply.User.HeadImage.Length];
                                reply.User.HeadImage.CopyTo(GameState.currentUserHeadImage, 0);
                            }

                            ImageDownloadState state = new ImageDownloadState();
                            NetworkThread.SetGameState(state);
                            state.SendImageInfo();

                            return true;
                        }
                        else
                        {
                            currentWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                                {
                                    MessageBox.Show("用户名或密码错误");
                                }
                            );
                        }
                    }
                    catch (InvalidProtocolBufferException e)
                    {
                        Console.WriteLine("WriteLine failed as {0}", e.Message);
                    }
                    break;

                case MessageType.MSG_ECHO:
                    HandleEcho(msg);
                    break;

                default:
                    break;
            }

            return true;
        }

        public void SendRegist(String user, String pasd)
        {
            Register.Builder builder = new Register.Builder();
            builder.SetEmailAccount(user);
            builder.SetPassword(pasd);
            builder.SetUsername(user);

            Register regist = builder.Build();

            //序列化
            byte[] buf = regist.ToByteArray();
            NetworkThread.SendMessage(MessageType.MSG_REGISTER, buf, false);
        }

        public void SendLoginReq(String user, String pasd)
        {
            LogOnorOut.Builder builder = new LogOnorOut.Builder();
            builder.SetAccount(user);
            builder.SetPassword(pasd);

            GameState.currentUserName = user;
            GameState.currentUserAccount = user;

            LogOnorOut login = builder.Build();

            //序列化
            byte[] buf = login.ToByteArray();
            NetworkThread.SendMessage(MessageType.MSG_LOGIN, buf);
        }

        public void FindUsrPassword(string email)
        {
            FindPassword.Builder builder = new FindPassword.Builder();
            builder.SetEmail(email);

            FindPassword pwd = builder.Build();
            //序列化
            byte[] buf = pwd.ToByteArray();
            NetworkThread.SendMessage(MessageType.MSG_FIND_PASSWORD, buf);
        }
    }
    
    class GameReadyState : GameState
    {
        public override bool MessageHandle(MessageType msg_type, byte[] msg)
        {
            switch (msg_type)
            {
                case MessageType.MSG_ECHO:
                    HandleEcho(msg);
                    break;

                case MessageType.MSG_GAME_HALL_SUMARY:
                    GameHallSumayHandle(msg);
                    break;

                case MessageType.MSG_HALL_INFO:
                    GameHallReplyHandle(msg);
                    break;

                case MessageType.MSG_REQUEST_PLAY_REPLY:
                    if (GameRequestPlayReplyHandle(msg))
                    {
                        NetworkThread.SetGameState(new GamePlayingState());
                    }
                    break;

                default:
                    break;
            }
            return true;
        }

        public void GameHallSumay()
        {
            GameHallSumaryReq.Builder builder = new GameHallSumaryReq.Builder();
            builder.SetOpcode(1);

            GameHallSumaryReq req = builder.Build();
            byte[] buf = req.ToByteArray();
            NetworkThread.SendMessage(MessageType.MSG_GAME_HALL_SUMARY_REQ, buf);
        }

        public void GameHallRequest(int hallID)
        {
            HallInfoReq.Builder builder = new HallInfoReq.Builder();
            builder.SetGameHallId(hallID);

            HallInfoReq req = builder.Build();
            byte[] buf = req.ToByteArray();
            NetworkThread.SendMessage(MessageType.MSG_HALL_INFO_REQ, buf);
        }

        public void RequestGamePlay(int hallID, int chessBoard, int l)
        {
            RequestPlay.Builder builder = new RequestPlay.Builder();

            gameHallID = hallID;
            builder.SetGameHallId(gameHallID);

            chessBoardID = chessBoard;
            builder.SetChessBoardId(chessBoardID);

            locate = l;
            builder.SetLocate(locate);

            RequestPlay req = builder.Build();
            byte[] buf = req.ToByteArray();
            NetworkThread.SendMessage(MessageType.MSG_REQUEST_PLAY, buf);
        }

        public bool GameRequestPlayReplyHandle(byte[] msg)
        {
            bool ret = false;
            RequestPlayReply reply = RequestPlayReply.ParseFrom(msg);
            Console.WriteLine("status = "+ reply.Status);
            Console.WriteLine("first come user locate : " + reply.FirstComeUserLocate);
            ChessBoard.firsr_come_user_locate = (int)reply.FirstComeUserLocate;

            if (reply.Status == 1)
            {
                gameHallWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                                {
                                    Chess chess = new Chess();
                                    //chess.Owner = currentWin;
                                    GameState.SetCurrentWin(chess);
                                    gameWin = chess;
                                    chess.Show();
                                }
                            );

                ret = true;
            }

            if (reply.HasChessBoard)
            {
                chessBoardID = (int)reply.ChessBoard.Id;
                Console.WriteLine("chessBoard.id = " + reply.ChessBoard.Id);
                Console.WriteLine("chessBoard.people_num = " + reply.ChessBoard.PeopleNum);
                Console.WriteLine("chessBoard.left_user.chess_board_empty = " + reply.ChessBoard.LeftUser.ChessBoardEmpty);
                Console.WriteLine("chessBoard.right_user.chess_board_empty = " + reply.ChessBoard.RightUser.ChessBoardEmpty);
                Console.WriteLine("chessBoard.bottom_user.chess_board_empty = " + reply.ChessBoard.BottomUser.ChessBoardEmpty);
            }

            return ret;
        }

        public void UpdateUserInfos(string account, string name, string pwd, string email="", string phone = "", byte[] headImage = null)
        {
            UpdateUserInfo.Builder builder = new UpdateUserInfo.Builder();
            builder.SetAccount(account);
            builder.SetUserName(name);
            builder.SetPassword(pwd);

            GameState.currentUserName = name;
            //Display user nick name and score
            GameState.gameHallWin.nick_name_lab.Content = GameState.currentUserName;

            if ((email != null) && (email.Length > 0))
            {
                builder.SetExEmail(email);
                GameState.currentUserEmail = email;
            }

            if ((phone != null) && (phone.Length > 0))
            {
                builder.SetPhone(phone);
                GameState.currentUserPhoneNo = phone;
            }

            if ((headImage != null) && (headImage.Length > 0))
            {
                /*FileStream fs = File.OpenRead(@"C:\Users\GBX386\Desktop\Visual C#\Image\按用户统计.png");
                Byte[] Buff = new Byte[fs.Length];
                fs.Read(Buff, 0, Buff.Length);
                fs.Close();
                
                Console.WriteLine("Read image length = " + Buff.Length.ToString());*/
                MD5Hash hash = new MD5Hash();
                string hashCode = hash.GetMd5Hash(headImage);
                if (true != hash.VerifyMd5Hash(GameState.currentUserHeadImage, hashCode))
                {
                    builder.SetHeadImage(ByteString.CopyFrom(headImage));
                    GameState.currentUserHeadImage = headImage;
                    GameState.gameHallWin.DisplayHeadImage();
                }
            }

            UpdateUserInfo info = builder.Build();
            byte[] buf = info.ToByteArray();
            NetworkThread.SendMessage(MessageType.MSG_UPDATE_USER_INFO, buf);
        }
    }

    class GamePlayingState : GameState
    {
        public override bool MessageHandle(MessageType msg_type, byte[] msg)
        {
            bool ret = true;

            switch (msg_type)
            {
                case MessageType.MSG_ECHO:
                    HandleEcho(msg);
                    break;

                case MessageType.MSG_GAME_HALL_SUMARY:
                    GameHallSumayHandle(msg);
                    break;

                case MessageType.MSG_HALL_INFO:
                    GameHallReplyHandle(msg);
                    break;

                case MessageType.MSG_ANNOUNCE_MOVE:
                    ParseAnnounceMoveReply(msg);
                    break;
                case MessageType.MSG_USER_MSG:
                    ParseIMMsgReply(msg);
                    break;
                case MessageType.MSG_GAME_STATUS:
                    ParseGameStatusReply(msg);
                    break;
                case MessageType.MSG_GIVE_UP:
                    ParseLeaveOutRoomMsg(msg);
                    break;
                case MessageType.MSG_CHESS_BOARD:
                    ParseChessBoard(msg);
                    break;

                default:
                    break;
            }

            return ret;
        }

        public bool ParseChessBoard(byte[] msg)
        {
            chessBoard = ChessBoardInfo.ParseFrom(msg);

            Console.WriteLine("ChessBoardID: " + chessBoard.Id);
            Console.WriteLine("PeopleNum: " + chessBoard.PeopleNum);
            Console.WriteLine("LeftUser.ChessBoardEmpty: " + chessBoard.LeftUser.ChessBoardEmpty);
            if (!chessBoard.LeftUser.ChessBoardEmpty)
            {
                Console.WriteLine("LeftUser.Account: " + chessBoard.LeftUser.Account);
                Console.WriteLine("LeftUser.UserName: " + chessBoard.LeftUser.UserName);
                Console.WriteLine("LeftUser.Score: " + chessBoard.LeftUser.Score);
                Console.WriteLine("LeftUser.Status: " + chessBoard.LeftUser.Status);
            }

            Console.WriteLine("RightUser.ChessBoardEmpty: " + chessBoard.RightUser.ChessBoardEmpty);
            if (!chessBoard.RightUser.ChessBoardEmpty)
            {
                Console.WriteLine("RightUser.Account: " + chessBoard.RightUser.Account);
                Console.WriteLine("RightUser.UserName: " + chessBoard.RightUser.UserName);
                Console.WriteLine("RightUser.Score: " + chessBoard.RightUser.Score);
                Console.WriteLine("RightUser.Status: " + chessBoard.RightUser.Status);
            }

            Console.WriteLine("BottomUser.ChessBoardEmpty: " + chessBoard.BottomUser.ChessBoardEmpty);
            if (!chessBoard.BottomUser.ChessBoardEmpty)
            {
                Console.WriteLine("BottomUser.Account: " + chessBoard.BottomUser.Account);
                Console.WriteLine("BottomUser.UserName: " + chessBoard.BottomUser.UserName);
                Console.WriteLine("BottomUser.Score: " + chessBoard.BottomUser.Score);
                Console.WriteLine("BottomUser.Status: " + chessBoard.BottomUser.Status);
            }
            
            if (allUsersReady)
            {
                gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                                {
                                    gameWin.ChessBoardLoaded(null, null);
                                    User user = ChessBoard.GetChessBoardObj().GetUserByUsrLocation((Location)currentTokenLocate);
                                    if (user != null)
                                    {
                                        user.timer.Start();
                                    }

                                    ChessBoard.GetChessBoardObj().leftUser.account = chessBoard.LeftUser.Account;
                                    ChessBoard.GetChessBoardObj().leftUser.user_name = chessBoard.LeftUser.UserName;
                                    ChessBoard.GetChessBoardObj().leftUser.score = chessBoard.LeftUser.Score;

                                    ChessBoard.GetChessBoardObj().righttUser.account = chessBoard.RightUser.Account;
                                    ChessBoard.GetChessBoardObj().righttUser.user_name = chessBoard.RightUser.UserName;
                                    ChessBoard.GetChessBoardObj().righttUser.score = chessBoard.RightUser.Score;

                                    ChessBoard.GetChessBoardObj().bottomUser.account = chessBoard.BottomUser.Account;
                                    ChessBoard.GetChessBoardObj().bottomUser.user_name = chessBoard.BottomUser.UserName;
                                    ChessBoard.GetChessBoardObj().bottomUser.score = chessBoard.BottomUser.Score;

                                    ChessBoard.GetChessBoardObj().currentUser.user_name = GameState.currentUserName;
                                }
                            );              
            }

            return true;
        }

        public void SendChessBoardReq()
        {
            ChessBoardInfoReq chessBoard;
            ChessBoardInfoReq.Builder builder = new ChessBoardInfoReq.Builder();

            builder.SetChessBoardId(chessBoardID);
            chessBoard = builder.Build();
            byte[] bytes = chessBoard.ToByteArray();

            NetworkThread.SendMessage(MessageType.MSG_CHESS_BOARD_REQ, bytes);
        }

        public void LeaveOutFromRoom()
        {
            GiveUp giveUp;
            GiveUp.Builder builder = new GiveUp.Builder();
            builder.SetSrcUserLocate((uint)locate);

            giveUp = builder.Build();

            byte[] bytes = giveUp.ToByteArray();

            NetworkThread.SendMessage(MessageType.MSG_GIVE_UP, bytes);
            NetworkThread.SetGameState(new GameReadyState());
        }

        private bool ParseLeaveOutRoomMsg(byte[] msg)
        {
            GiveUp giveUp = GiveUp.ParseFrom(msg);
            User usr = null;
            //
            try
            {
                usr = ChessBoard.GetChessBoardObj().GetUserByUsrLocation((Location)giveUp.SrcUserLocate);
                usr.State = User.GameState.LOSE;
            }
            catch (Exception e)
            {
                MessageBox.Show("Get The User object failed! " + e.Message);
                return false;
            }

            gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                MessageBox.Show(usr.user_name +" left out the game room, the game will end!");
            }
            );
            return true;
        }

        public void ParseAnnounceMoveReply(byte [] msg)
        {
            MoveAction reply = MoveAction.ParseFrom(msg);
            currentTokenLocate = (Location)(reply.TokenLocate);

            Console.WriteLine("src_user_locat :" + reply.SrcUserLocate);
            Console.WriteLine("TokenLocate :" + reply.TokenLocate);
            Console.WriteLine("SrcChessType :" + reply.Movechess.SrcChessType);
            Console.WriteLine("SrcUserLocate :" + reply.Movechess.SrcUserLocate);
            Console.WriteLine("IsWinner :" + reply.Movechess.IsWinner);
            Console.WriteLine("FromPointX :" + reply.Movechess.FromPointX);
            Console.WriteLine("FromPointY :" + reply.Movechess.FromPointY);
            Console.WriteLine("DesPointX :" + reply.Movechess.DesPointX);
            Console.WriteLine("DesPointY :" + reply.Movechess.DesPointY);


            gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                {
                    if (reply.SrcUserLocate != locate)
                    {
                        ChessBoard.handler_chess_move(reply.Movechess.FromPointX, reply.Movechess.FromPointY,
                            reply.Movechess.DesPointX, reply.Movechess.DesPointY);
                    }
                    //注意，该条消息时该棋盘广播的消息，三方都可以接受消息，但只有另外两方需要走子
                    //暂停当前走棋用户的定时器，并开启下一个应该走子用户的定时器
                    User user = ChessBoard.GetChessBoardObj().GetUserByUsrLocation((Location)reply.SrcUserLocate);
                    if (user != null)
                    {
                        user.timer.Stop();
                    }

                    user = ChessBoard.GetChessBoardObj().GetUserByUsrLocation((Location)reply.TokenLocate);
                    if (user != null)
                    {
                        user.timer.Start();
                    }
                }
            );
        }

        public bool MoveChessReq(int chessType, int srcLocate, int fromX, int fromY, int desX, int desY, bool win, 
            int tarChessType=-1, int tarLocate=-1)
        {
            MoveChess moveChess;
            MoveChess.Builder builder = new MoveChess.Builder();
            builder.SetSrcChessType(chessType);
            builder.SetSrcUserLocate(srcLocate);
            builder.SetFromPointX(fromX);
            builder.SetFromPointY(fromY);
            builder.SetDesPointX(desX);
            builder.SetDesPointY(desY);
            builder.SetIsWinner(win);

            builder.SetTargetUserLocate((uint)tarLocate);
            builder.SetEatTargetChessType(tarChessType);

            moveChess = builder.Build();
            byte[] bytes = moveChess.ToByteArray();

            NetworkThread.SendMessage(MessageType.MSG_MOVE_CHESS, bytes);

            return true;
        }

        public void ParseIMMsgReply(byte[] msg)
        {
            UserMessage reply = UserMessage.ParseFrom(msg);
            Console.WriteLine("src_user_locate: " + reply.SrcUserLocate);
            Console.WriteLine("msg content: " + reply.MsgContent);

            gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    gameWin.AddMsgToBox(ChessBoard.GetChessBoardObj().GetUserByUsrLocation((Location)reply.SrcUserLocate).user_name,
                        reply.MsgContent);
                }
            );
        }

        public bool SendIMMessage(string im_msg)
        {
            UserMessage msg;
            UserMessage.Builder builder = new UserMessage.Builder();
            builder.SetMsgContent(im_msg);
            builder.SetSrcUserLocate((uint)GameState.locate);

            msg = builder.Build();
            byte[] bytes = msg.ToByteArray();

            NetworkThread.SendMessage(MessageType.MSG_USER_MSG, bytes);

            return true;
        }

        public void StartGameReadyReq(int total_sec = 0, int step_sec = 0)
        {
            GameReadyReq.Builder builder = new GameReadyReq.Builder();
            builder.SetSrcUserLocate((uint)GameState.locate);

            if (GameState.locate == ChessBoard.firsr_come_user_locate)
            {
                //设置参数
                builder.SetTotalTime((uint)total_sec);
                builder.SetSingleStepTime((uint)step_sec);
            }

            GameReadyReq msg = builder.Build();
            byte[] bytes = msg.ToByteArray();

            NetworkThread.SendMessage(MessageType.MSG_GAME_READY_REQ, bytes);
        }

        public bool ParseGameStatusReply(byte[] msg)
        {
            GameStatusReply reply = GameStatusReply.ParseFrom(msg);
            bool ret = false;

            currentTokenLocate = (Location)(reply.TokenLocate);
            if ((reply.LeftUserStatus == true) &&
                (reply.RightUserStatus == true) &&
                (reply.BottomUserStatus == true) &&
                (reply.TotalTime != 0) &&
                (reply.SingleStepTime != 0))
            {
                Console.WriteLine("All Users have been ready now.\n" + "Current {0} User should go", reply.TokenLocate);                
                allUsersReady = true;
                ChessBoard.total_time = (int)reply.TotalTime;
                ChessBoard.single_step_time = (int)reply.SingleStepTime;

                GamePlayingState state = new GamePlayingState();
                state.SendChessBoardReq();

                ret = true;
            }
            else
            {
                Console.WriteLine("Not all users have been ready.");
            }

            gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                                {
                                    if (gameWin.pressGameReadyBtn && gameWin.StartGameBtn.IsEnabled)
                                    {
                                        gameWin.StartGameBtn.IsEnabled = false;
                                    }
                                }
                            );
            
            return ret;
        }
    }


    public struct ImageItemInfo
    {
        public AdPictureItemReply item;
        public string locate_path;
    }

    public class ImageIDLocateMap
    {
        public ImageID imageID;
        public string locate_path;
        public string link_url;
        public uint size = 0;
        public string type = "";
        public bool existed = false;

        public ImageIDLocateMap(ImageID id, string str)
        {
            this.imageID = id;
            this.locate_path = str;
        }
    }

    class ImageDownloadState : GameState
    {
        private string currentFilePath = "";
        private const string PathPrefix = @"C:\Users\GBX386\Desktop\Visual C#\WpfApplication2\WpfApplication2\Images\AdPictures\";
        private FileStream fs = null;
        private int currentImageID = (int)ImageID.IMAGE_ID_AD_HALL_TOP_LEFT - 1;

        public static ImageIDLocateMap[] ImageIDMap = new ImageIDLocateMap[] { 
            new ImageIDLocateMap (ImageID.IMAGE_ID_AD_HALL_TOP_LEFT,    PathPrefix),
            new ImageIDLocateMap (ImageID.IMAGE_ID_AD_HALL_TOP_RIGHT,   PathPrefix),
            new ImageIDLocateMap (ImageID.IMAGE_ID_AD_HALL_BOTTOM_LEFT, PathPrefix),
            new ImageIDLocateMap (ImageID.IMAGE_ID_AD_HALL_BOTTOM_RIGHT, PathPrefix),
            new ImageIDLocateMap (ImageID.IMAGE_ID_AD_BOARD_TOP_LEFT, PathPrefix),
            new ImageIDLocateMap (ImageID.IMAGE_ID_AD_BOARD_TOP_RIGHT, PathPrefix),
            new ImageIDLocateMap (ImageID.IMAGE_ID_AD_BOARD_BOTTOM_LEFT, PathPrefix),
            new ImageIDLocateMap (ImageID.IMAGE_ID_AD_BOARD_BOTTOM_RIGHT, PathPrefix),
            new ImageIDLocateMap (ImageID.IMAGE_ID_AD_BOARD_MIDDLE, PathPrefix),
            new ImageIDLocateMap (ImageID.IMAGE_ID_LOGIN_BACKGROUND, PathPrefix)};

        public override bool MessageHandle(MessageType msg_type, byte[] msg)
        {
            bool ret = true;

            switch (msg_type)
            {
                case MessageType.MSG_ECHO:
                    HandleEcho(msg);
                    break;

                case MessageType.MSG_GAME_HALL_SUMARY:
                    GameHallSumayHandle(msg);
                    break;

                case MessageType.MSG_HALL_INFO:
                    GameHallReplyHandle(msg);
                    break;

                case MessageType.MSG_AD_IMAGE_CONTENT:
                    HandleImageContent(msg);
                    break;
                case MessageType.MSG_AD_IMAGE_INFO:
                    HandleImageInfo(msg);
                    break;

                default:
                    break;
            }

            return ret;
        }

        public void HandleImageContent(byte[] msg)
        {
            //chessBoard = ChessBoardInfo.ParseFrom(msg);
            AdPictureContentReply reply = AdPictureContentReply.ParseFrom(msg);
            if (reply.Synced && (!reply.HasEnded || (reply.HasEnded && !reply.Ended)))
            {
                if (fs == null)
                {
                    fs = File.OpenWrite(currentFilePath);
                    fs.Position = 0;
                    //recv_length = 0;
                }

                if (reply.HasContent)
                {
                    //Console.WriteLine("Write content length=" + reply.Content.Length.ToString() + "  recv_length:" + recv_length.ToString() + " total:" + ImageIDMap[currentImageID].size.ToString());
                    fs.Write(reply.Content.ToByteArray(), 0, reply.Content.Length);
                }  
            }
            else
            {
                if (reply.HasEnded)
                {
                    if (reply.Ended)
                    {
                        if (fs != null)
                        {
                            fs.Close();
                            fs = null;
                        }
                    }
                    else
                    {
                        if (fs != null)
                        {
                            fs.Close();
                            fs = null;
                        }
                    }
                }
            }

            if ((reply.HasEnded && reply.Ended) && (!reply.Synced))
            {
                currentWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate()
                    {
                        Window1 gameHall = new Window1();
                        currentWin.Hide();
                        gameHall.Owner = currentWin;
                        logginWin = (MainWindow)currentWin;
                        SetCurrentWin(gameHall);
                        SetGameHallWin(gameHall);
                        gameHall.ShowDialog();

                        NetworkThread.DestroryWorkThread();
                        logginWin.Close();
                    }
                );

                NetworkThread.SetGameState(new GameReadyState());
                EchoRequest.TimerTest();

                GameReadyState ready = new GameReadyState();
                ready.GameHallSumay();
            }
            else if ((reply.HasEnded && reply.Ended) && (reply.Synced))
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                    //recv_length = 0;
                }

                GetNextFile();
            }
        }

        public void HandleImageInfo(byte[] msg)
        {
            AdPictureItemReply currPicItemReply = AdPictureItemReply.ParseFrom(msg);

            if (currPicItemReply.Existed)
            {
                ImageIDMap[currPicItemReply.ImageId].locate_path += currPicItemReply.ImageName;
                ImageIDMap[currPicItemReply.ImageId].link_url = currPicItemReply.Url;
                ImageIDMap[currPicItemReply.ImageId].size = currPicItemReply.ImageSize;
                ImageIDMap[currPicItemReply.ImageId].type = currPicItemReply.ImageType;
                ImageIDMap[currPicItemReply.ImageId].existed = currPicItemReply.Existed;

                currentFilePath = ImageIDMap[currPicItemReply.ImageId].locate_path;
                Console.WriteLine("Current file : " + currentFilePath + "  Size:" + ImageIDMap[currPicItemReply.ImageId].size.ToString());

                string hashcode = GetFileHashCode(currentFilePath);
                if (hashcode == null || hashcode.Length <= 0)
                {
                    Console.WriteLine("Open file failed!!!  Maybe It's not exsited, Now get it down!");
                    WrapImageReq((ImageID)currPicItemReply.ImageId, "00");
                }
                else
                {
                    if (hashcode.Equals(currPicItemReply.ImageHashcode))
                    {
                        //Get next file
                        GetNextFile();
                    }
                    else
                    {
                        WrapImageReq((ImageID)currPicItemReply.ImageId, "00");
                    }
                }
            }
            else
            {
                //Get next file
                GetNextFile();
            }
        }

        private string GetFileHashCode(string path)
        {
            string hashcode = "";

            FileStream file = null;

            try
            {
                file = File.OpenRead(path);
                if (file != null)
                {
                    file.Position = 0;
                    MD5Hash hash = new MD5Hash();
                    hashcode = hash.GetMd5Hash(file);
                    Console.WriteLine("Got the md5:{0} for {1}", hashcode, currentFilePath);
                    file.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Can NOT open for " + e.Message);
            }

            return hashcode;
        }

        public void SendImageInfo()
        {
            ++currentImageID;
            WrapImageReq((ImageID)currentImageID);
        }

        private void GetNextFile()
        {
            ++currentImageID;
            if (currentImageID < (int)ImageID.IMAGE_ID_MAX)
            {
                if (currentImageID < (int)ImageID.IMAGE_ID_MAX - 1)
                    WrapImageReq((ImageID)currentImageID);
                else
                    WrapImageReq((ImageID)currentImageID, "", true);
            }
        }

        private void WrapImageReq(ImageID id, string hash="", bool last=false)
        {
            AdPictureReq picItem;
            AdPictureReq.Builder builder = new AdPictureReq.Builder();

            builder.SetImageId((uint)id);
            if ((hash != null) && (hash.Length > 0))
            {
                builder.SetImageHashcode(hash);
            }

            builder.SetLastOne(last);

            picItem = builder.Build();
            byte[] bytes = picItem.ToByteArray();

            NetworkThread.SendMessage(MessageType.MSG_AD_IMAGE_REQ, bytes);
        }
    }

    class EchoRequest
    {
        private int invokeCount;
        private int maxCount;
        private int failedCount;

        public EchoRequest()
        {
            invokeCount = 0;
            failedCount = 0;
            maxCount = 10;
        }

        public static void TimerTest()
        {
            EchoRequest echoReq = new EchoRequest();
            TimerCallback tcb = echoReq.SendEchoRequest;
            Timer t = new Timer(new TimerCallback(echoReq.SendEchoRequest));
            t.Change(1000, 1000 * 50);
        }
        // This method is called by the timer delegate.
        public void SendEchoRequest(Object stateInfo)
        {
            Timer t = (Timer)stateInfo;
            Console.WriteLine("{0} SendEchoRequest {1,2}.", DateTime.Now.ToString("h:mm:ss.fff"), (++invokeCount).ToString());

            Echo.Builder builder = new Echo.Builder();
            builder.SetTimeStamp(DateTime.Now.ToString("h:mm:ss.fff"));

            try
            {
                Echo echoInfo = builder.Build();

                //序列化
                byte[] buf = echoInfo.ToByteArray();
                NetworkThread.SendMessage(MessageType.MSG_ECHO, buf, false);
            }
            catch (Exception e)
            {
                failedCount++;
                Console.WriteLine("Send failed for " + e.Message);

                if (failedCount == maxCount)
                    t.Dispose();
                else
                    t.Change(60 * 1000, 60 * 1000);
            }

        }
    }
}

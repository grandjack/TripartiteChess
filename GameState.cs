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
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

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

    public enum UserStatus
    {
        STATUS_NOT_START = 0,
        STATUS_EXITED = STATUS_NOT_START,
        STATUS_READY,
        STATUS_PLAYING,
        STATUS_ENDED 
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

        static public ChessBoardUser gLeftUser = null;
        static public ChessBoardUser gRightUser = null;
        static public ChessBoardUser gBottomUser = null;
        static public ChessBoardUser gCurrentUser = null;

        static public UserStatus gCurrUserGameStatus = UserStatus.STATUS_NOT_START;

        static public string gWorkPath = AppDomain.CurrentDomain.BaseDirectory;

        static public int currentAgreeHeQiNum = 0;

        static public string defaultHeadImagePath = gWorkPath + @"\res\Images\MyIcon.png";

        static public void SetCurrentWin(Window win)
        {
            currentWin = win;
        }
        static public void SetLogWin(Window win)
        {
            logginWin = (MainWindow)win;
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

        public static ChessBoardUser GetCurrentUser()
        {
             switch((Location)locate)
            {
                 case Location.left:
                    gCurrentUser = gLeftUser;    break;
                 case Location.right:
                     gCurrentUser = gRightUser; break;
                 case Location.bottom:
                     gCurrentUser = gBottomUser; break;
            }

            return gCurrentUser;
        }

        public static int GetActiveUserNum()
        {
            int num = 0;
            if ((gLeftUser != null) && 
                (!gLeftUser.ChessBoardEmpty) &&
                (gLeftUser.HasStatus) &&
                (gLeftUser.Status == (uint)UserStatus.STATUS_PLAYING))
            {
                ++num;
            }

            if ((gRightUser != null) &&
                (!gRightUser.ChessBoardEmpty) &&
                (gRightUser.HasStatus) &&
                (gRightUser.Status == (uint)UserStatus.STATUS_PLAYING))
            {
                ++num;
            }

            if ((gBottomUser != null) &&
                (!gBottomUser.ChessBoardEmpty) &&
                (gBottomUser.HasStatus) &&
                (gBottomUser.Status == (uint)UserStatus.STATUS_PLAYING))
            {
                ++num;
            }

            return num;
        }

        public static ChessBoardUser GetSpeciUserFromHall(int chessBoadrIndex, Location locate)
        {
            ChessBoardUser user = null;
            if (GameState.hallInfo != null)
            {
                ChessBoardInfo info = GameState.hallInfo.GetChessBoard((int)(chessBoadrIndex - 1));
                if (info != null)
                {
                    switch (locate)
                    {
                        case Location.left:
                            user = info.LeftUser;break;
                        case Location.right:
                            user = info.RightUser;break;
                        case Location.bottom:
                            user = info.BottomUser;break;
                        default:
                            break;
                    }
                }
            }

            return user;
        }

        public static ChessBoardUser GetChessBoardtUserByLocate(uint location)
        {
            ChessBoardUser usr = null;

            switch ((Location)location)
            {
                case Location.left:
                    usr = gLeftUser; break;
                case Location.right:
                    usr = gRightUser; break;
                case Location.bottom:
                    usr = gBottomUser; break;
            }

            return usr;
        }

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
                        if (gameHallWin.send_quick_start)
                        {
                            gameHallWin.QuickStart(null, null);
                        }
                    }
                );
            }
            catch (Exception e)
            {
                Console.WriteLine("Parse failed for " + e.Message);
            }

            return true;
        }

        public bool GameRequestPlayReplyHandle(byte[] msg)
        {
            bool ret = false;
            RequestPlayReply reply = RequestPlayReply.ParseFrom(msg);
            Console.WriteLine("status = " + reply.Status);
            Console.WriteLine("first come user locate : " + reply.FirstComeUserLocate);
            ChessBoard.firsr_come_user_locate = (int)reply.FirstComeUserLocate;

            if (reply.HasChessBoard)
            {
                chessBoardID = (int)reply.ChessBoard.Id;
                Console.WriteLine("chessBoard.id = " + reply.ChessBoard.Id);
                Console.WriteLine("chessBoard.people_num = " + reply.ChessBoard.PeopleNum);
                Console.WriteLine("chessBoard.left_user.chess_board_empty = " + reply.ChessBoard.LeftUser.ChessBoardEmpty);
                Console.WriteLine("chessBoard.right_user.chess_board_empty = " + reply.ChessBoard.RightUser.ChessBoardEmpty);
                Console.WriteLine("chessBoard.bottom_user.chess_board_empty = " + reply.ChessBoard.BottomUser.ChessBoardEmpty);

                if (!reply.ChessBoard.LeftUser.ChessBoardEmpty)
                {
                    gLeftUser = reply.ChessBoard.LeftUser;
                }

                if (!reply.ChessBoard.RightUser.ChessBoardEmpty)
                {
                    gRightUser = reply.ChessBoard.RightUser;
                }

                if (!reply.ChessBoard.BottomUser.ChessBoardEmpty)
                {
                    gBottomUser = reply.ChessBoard.BottomUser;
                }
            }

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
            else if (reply.Status == 2)
            {
                gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                                {
                                    gameWin.UsersInfoLoadAndUpdate();
                                }
                            );
            }

            return ret;
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
                        logginWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                 (ThreadStart)delegate()
                                 {
                                     logginWin.click_count = 0;
                                 }
                        );

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
                            NetworkThread.DestroryWorkThread();
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
                        logginWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                 (ThreadStart)delegate()
                                 {
                                     logginWin.click_count = 0;
                                 }
                             );
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

                            bool ret = IniFileHand.WriteIniData("User", "Account", GameState.currentUserAccount, GameState.gWorkPath + @"\res\files\info.ini");
                            if (ret)
                            {
                                Console.WriteLine("Write the config account:" + GameState.currentUserAccount);
                            }

                            ImageDownloadState state = new ImageDownloadState();
                            NetworkThread.SetGameState(state);
                            state.SendImageInfo();

                            return true;
                        }
                        else
                        {
                            logginWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                                {
                                    MessageBox.Show("用户名或密码错误");
                                }
                            );
                            NetworkThread.DestroryWorkThread();
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
                        GameState.gCurrUserGameStatus = UserStatus.STATUS_READY;
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
        static public Undo gUndo_msg = null;

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
                case MessageType.MSG_REQUEST_PLAY_REPLY:
                    GameRequestPlayReplyHandle(msg);
                    break;

                case MessageType.MSG_UNDO_REPS:
                case MessageType.MSG_UNDO_REQ:
                    UndoParseReply(msg);
                    break;

                case MessageType.MSG_RECONCILED_REQ:
                case MessageType.MSG_RECONCILED_RESP:
                    HeQiParse(msg);
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

            if (chessBoard.HasLeftUser)
            {
                gLeftUser = chessBoard.LeftUser;
            } else {
                gLeftUser = null;
            }
            if (chessBoard.HasRightUser)
                gRightUser = chessBoard.RightUser;
            else
                gRightUser = null;

            if (chessBoard.HasBottomUser)
                gBottomUser = chessBoard.BottomUser;
            else
                gBottomUser = null;

            gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (ThreadStart)delegate()
                                {
                                    gameWin.UsersInfoLoadAndUpdate();
                                }
            );


            if (GameState.currentUserScore != GetCurrentUser().Score)
            {
                GameState.currentUserScore = GetCurrentUser().Score;
                gameHallWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate()
                {
                    gameHallWin.score_lab.Content = GameState.currentUserScore;
                }
                );
            }

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

        public void LeaveOutFromRoom(string opt=null)
        {
            GiveUp giveUp;
            GiveUp.Builder builder = new GiveUp.Builder();
            builder.SetSrcUserLocate((uint)locate);

            if ((opt != null ) && (opt.Length>0)) 
            {
                builder.SetOpt(opt);
                NetworkThread.SetGameState(new GameReadyState());
            }

            giveUp = builder.Build();

            byte[] bytes = giveUp.ToByteArray();

            NetworkThread.SendMessage(MessageType.MSG_GIVE_UP, bytes);
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
                Console.WriteLine("Get The User object failed! " + e.Message);
                return false;
            }

            ChessBoardUser give_up_usr = GameState.GetChessBoardtUserByLocate(giveUp.SrcUserLocate);
            gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                ChessBoard.GetChessBoardObj().gGameStatus = ChessBoard.GameSatus.END;
                MessageBox.Show(give_up_usr.UserName + " 退出游戏，本轮游戏结束！", "提示");
                try{
                    ChessBoard.GetChessBoardObj().leftUser.timer.Stop();
                    ChessBoard.GetChessBoardObj().righttUser.timer.Stop();
                    ChessBoard.GetChessBoardObj().bottomUser.timer.Stop();
                } catch(Exception e)
                {
                    Console.WriteLine("Game over! Stop timer failed.");
                }
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
                    GameState.currentTokenLocate = (Location)(reply.TokenLocate);
                    if (reply.SrcUserLocate != locate)
                    {
                        ChessBoard.handler_chess_move(reply.Movechess.FromPointX, reply.Movechess.FromPointY,
                            reply.Movechess.DesPointX, reply.Movechess.DesPointY);
                    }
                    else
                    {
                        //设置可以悔棋状态
                        Console.WriteLine("Could set HuiQi button enable ! currentTokenLocate=" + currentTokenLocate.ToString());
                        ChessBoard.justGo = true;
                        gameWin.SetHuiQiButtonStatus(true);
                    }

                    if (GameState.currentTokenLocate == (Location)GameState.locate)
                    {
                        if (gameWin.HeQiBtn.IsEnabled == false)
                        {
                            gameWin.SetQiuHeButtonStatus(true);
                        }

                        if (!gameWin.RenShuBtn.IsEnabled && (GameState.GetActiveUserNum() == 2))
                        {
                            gameWin.SetRenShuButtonStatus(true);
                        }
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
                GameState.gCurrUserGameStatus = UserStatus.STATUS_PLAYING;

                gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    gameWin.ReMoveMiddleAd();
                    gameWin.ChessBoardLoaded(null, null);
                    User user = ChessBoard.GetChessBoardObj().GetUserByUsrLocation((Location)currentTokenLocate);
                    if (user != null)
                    {
                        user.timer.Start();
                    }
                    MediaPlayer player = new MediaPlayer();
                    player.Open(new Uri(GameState.gWorkPath + @"\res\voice\Begin.wav", UriKind.Absolute));
                    player.Play();

                }
                );

                SendChessBoardReq();
                
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
                                        gameWin.SetStartButtonStatus(false);
                                    }
                                }
                            );
            
            return ret;
        }

        //请求悔棋
        public void UndoRequest()
        {
            if (currentTokenLocate == Location.unknown)
            {
                Console.WriteLine("Current token locate is unknown, can NOT request HuiQi");
                return;
            }
            Undo.Builder builder = new Undo.Builder();
            builder.SetRepOrRespon(0);
            builder.SetSrcUserLocate((uint)GameState.locate);
            builder.SetTarUserLocate((uint)currentTokenLocate);

            Undo msg = builder.Build();
            byte[] bytes = msg.ToByteArray();

            NetworkThread.SendMessage(MessageType.MSG_UNDO_REQ, bytes);
        }

        //回复悔棋请求
        public void UndoRequestReply(bool agree)
        {
            Undo.Builder builder = new Undo.Builder();
            builder.SetRepOrRespon(1);
            builder.SetSrcUserLocate(undo_req_src_locate);
            builder.SetTarUserLocate(undo_req_tar_locate);
            builder.SetStatus(agree);

            Undo msg = builder.Build();
            byte[] bytes = msg.ToByteArray();

            NetworkThread.SendMessage(MessageType.MSG_UNDO_REPS, bytes);
        }

        static private uint undo_req_src_locate = 3;
        static private uint undo_req_tar_locate = 3;
        public bool UndoParseReply(byte []msg)
        {
            gUndo_msg = Undo.ParseFrom(msg);
            Console.WriteLine("Receive Undo data!");
            Console.WriteLine("RepOrRespon=" + gUndo_msg.RepOrRespon);
            Console.WriteLine("TarUserLocate=" + gUndo_msg.TarUserLocate);
            Console.WriteLine("SrcUserLocate=" + gUndo_msg.SrcUserLocate);
            if (gUndo_msg.HasStatus)
            {
                Console.WriteLine("Status=" + gUndo_msg.Status);
            }

            if (gUndo_msg.RepOrRespon == 0)//这是一个请求报文
            {
                undo_req_src_locate = gUndo_msg.SrcUserLocate;
                undo_req_tar_locate = gUndo_msg.TarUserLocate;
                if (gUndo_msg.TarUserLocate == (uint)GameState.locate)//need give tip and select whether approve
                {
                    if (ChessBoard.justGo == false)//当前用户还没走子，则给用户提示是否同意悔棋
                    {
                        gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                        {
                            MessageBoxResult result = MessageBox.Show(GameState.GetChessBoardtUserByLocate(gUndo_msg.SrcUserLocate).UserName.ToString()
                                +" request to Hui Qi, Do you agree?", "Warnig", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                                {
                                    ChessBoard.HuiQi_chess_move();
                                }
                                );
                                //回复同意悔棋
                                GamePlayingState state = new GamePlayingState();
                                state.UndoRequestReply(true);
                            }
                            else
                            {
                                GamePlayingState state = new GamePlayingState();
                                state.UndoRequestReply(false);
                            }
                        }
                        );
                    } 
                    else//否则，系统默认拒绝悔棋
                    {
                        UndoRequestReply(false);
                    }
                }
            }
            else if (gUndo_msg.RepOrRespon == 1)//这是一个回复报文
            {
                if (gUndo_msg.SrcUserLocate == (uint)GameState.locate)//是我自己发送的请求回复报文
                {
                    if (gUndo_msg.HasStatus && gUndo_msg.Status)//允许悔棋
                    {
                        //HuiQi
                        gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                        {
                            ChessBoard.HuiQi_chess_move();
                        }
                        );
                        
                    }
                    else//对方拒绝悔棋
                    {
                        gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                        {
                            MessageBox.Show(GameState.GetChessBoardtUserByLocate(gUndo_msg.TarUserLocate).UserName.ToString()
                                + " reject HuiQi", "TiShi", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        );
                    }
                }
                else//是其他用户的请求回复报文，只需根据回复状态决定是否需要悔棋操作
                {
                    if (gUndo_msg.HasStatus && gUndo_msg.Status)//允许悔棋
                    {
                        //HuiQi
                        gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                        {
                            ChessBoard.HuiQi_chess_move();
                        }
                        );
                    }
                    else
                    {
                        //Do nothing!!!
                    }
                }
            }
            return true;
        }

        private static uint HeQiReqLocate = 3;
        //请求和棋
        public void QiuHeSendReq()
        {
            Reconciled.Builder builder = new Reconciled.Builder();
            builder.SetSrcUserLocate((uint)locate);
            builder.SetTarUserLocate((uint)currentTokenLocate);
            builder.SetApplyOrReply(0);

            Reconciled msg = builder.Build();
            byte[] bytes = msg.ToByteArray();
            NetworkThread.SendMessage(MessageType.MSG_RECONCILED_REQ, bytes);
        }
        //回复和棋请求
        public void QiuHeSendResponse(bool agree)
        {
            Reconciled.Builder builder = new Reconciled.Builder();
            builder.SetSrcUserLocate((uint)locate);
            builder.SetTarUserLocate((uint)HeQiReqLocate);
            builder.SetApplyOrReply(1);
            builder.SetStatus(agree.ToString());

            Reconciled msg = builder.Build();
            byte[] bytes = msg.ToByteArray();
            NetworkThread.SendMessage(MessageType.MSG_RECONCILED_RESP, bytes);
        }

        public bool HeQiParse(byte[] msg)
        {
            Reconciled qiuhe = Reconciled.ParseFrom(msg);
            Console.WriteLine("receive a He Qi request/reply");
            Console.WriteLine("qiuhe.ApplyOrReply:" + qiuhe.ApplyOrReply);
            Console.WriteLine("Src locate:" + qiuhe.SrcUserLocate);
            Console.WriteLine("Tar locate:" + qiuhe.TarUserLocate);

            if (qiuhe.ApplyOrReply == 0)//请求报文
            {
                HeQiReqLocate = qiuhe.SrcUserLocate; 
                
                gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                 (ThreadStart)delegate()
                                 {
                                     GamePlayingState.HeQiReqLocate = qiuhe.SrcUserLocate; 
                                     MessageBoxResult result= MessageBox.Show(GameState.GetChessBoardtUserByLocate(qiuhe.SrcUserLocate).UserName.ToString() + " 请求和棋","提示",MessageBoxButton.YesNo);
                                     if (result == MessageBoxResult.Yes)
                                     {
                                         GamePlayingState s = new GamePlayingState();
                                         s.QiuHeSendResponse(true);
                                     }
                                     else
                                     {
                                         GamePlayingState s = new GamePlayingState();
                                         s.QiuHeSendResponse(false);
                                     }
                                 }
                             );
            }
            else//回复报文
            {
                Console.WriteLine("qiuhe.Status:" + qiuhe.Status);
                if (qiuhe.Status.Equals("False"))
                {
                    gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                                {
                                    MessageBox.Show(GameState.GetChessBoardtUserByLocate(qiuhe.SrcUserLocate).UserName.ToString() + " 拒绝了您的请求！");
                                }
                            );
                }
                else
                {
                    gameWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                                {
                                    Console.WriteLine("GameState.currentAgreeHeQiNum:" + GameState.currentAgreeHeQiNum);
                                    Console.WriteLine("GameState.GetActiveUserNum()" + GameState.GetActiveUserNum());
                                    if (GameState.currentTokenLocate == (Location)locate)
                                    {
                                        GameState.currentAgreeHeQiNum++;
                                        MessageBox.Show(GameState.GetChessBoardtUserByLocate(qiuhe.SrcUserLocate).UserName.ToString() + " 同意了您的请求！");
                                        if (GameState.currentAgreeHeQiNum == GameState.GetActiveUserNum() - 1)
                                        {
                                            MessageBox.Show("和棋，本轮游戏结束！");
                                            ChessBoard.GetChessBoardObj().currentUser.timer.Stop();
                                        }
                                    }
                                }
                            );
                }
            }

            return true;
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
        private static string PathPrefix = GameState.gWorkPath + @"\res\Images\AdPictures\";
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

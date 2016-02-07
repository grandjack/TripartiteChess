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
        MSG_TYPE_MAX
    }

    abstract class GameState
    {
        static protected int gameHallID = 0;
        static protected int chessBoardID = 0;
        static protected int locate = 0;

        static public MainWindow logginWin;
        static public Window1 gameHallWin;
        static public Chess gameWin;
        static public Window currentWin;
        static public GameHallSumary sumary = null;
        static public HallInfo hallInfo = null;

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
                                        ((Window1)currentWin).HallListBoxLoaded(null, null);
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
                        Console.WriteLine("LeftUser.UserName: " + chessBoard.LeftUser.UserName);
                        Console.WriteLine("LeftUser.Score: " + chessBoard.LeftUser.Score);
                        Console.WriteLine("LeftUser.Status: " + chessBoard.LeftUser.Status);
                    }

                    Console.WriteLine("RightUser.ChessBoardEmpty: " + chessBoard.RightUser.ChessBoardEmpty);
                    if (!chessBoard.RightUser.ChessBoardEmpty)
                    {
                        Console.WriteLine("RightUser.UserName: " + chessBoard.RightUser.UserName);
                        Console.WriteLine("RightUser.Score: " + chessBoard.RightUser.Score);
                        Console.WriteLine("RightUser.Status: " + chessBoard.RightUser.Status);
                    }

                    Console.WriteLine("BottomUser.ChessBoardEmpty: " + chessBoard.BottomUser.ChessBoardEmpty);
                    if (!chessBoard.BottomUser.ChessBoardEmpty)
                    {
                        Console.WriteLine("BottomUser.UserName: " + chessBoard.BottomUser.UserName);
                        Console.WriteLine("BottomUser.Score: " + chessBoard.BottomUser.Score);
                        Console.WriteLine("BottomUser.Status: " + chessBoard.BottomUser.Status);
                    }
                }

                currentWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                    {
                        ((Window1)currentWin).game_hall_loaded(null, null);
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
                            currentWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                            {
                                Window1 gameHall = new Window1();
                                currentWin.Hide();
                                gameHall.Owner = currentWin;
                                logginWin = (MainWindow)currentWin;
                                SetCurrentWin(gameHall);
                                gameHall.ShowDialog();
                                NetworkThread.DestroryWorkThread();
                                logginWin.Close();
                            }
                            );

                            NetworkThread.SetGameState(new GameReadyState());
                            EchoRequest.TimerTest();

                            GameReadyState ready = new GameReadyState();
                            ready.GameHallSumay();
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

            LogOnorOut login = builder.Build();

            //序列化
            byte[] buf = login.ToByteArray();
            NetworkThread.SendMessage(MessageType.MSG_LOGIN, buf);
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
            if (reply.Status == 1)
            {
                currentWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate()
                                {
                                    Chess chess = new Chess();
                                    chess.Owner = currentWin;
                                    GameState.SetCurrentWin(chess);
                                    chess.Show();
                                }
                            );

                ret = true;
            }

            if (reply.HasChessBoard)
            {
                Console.WriteLine("chessBoard.id = " + reply.ChessBoard.Id);
                Console.WriteLine("chessBoard.people_num = " + reply.ChessBoard.PeopleNum);
                Console.WriteLine("chessBoard.left_user.chess_board_empty = " + reply.ChessBoard.LeftUser.ChessBoardEmpty);
                Console.WriteLine("chessBoard.right_user.chess_board_empty = " + reply.ChessBoard.RightUser.ChessBoardEmpty);
                Console.WriteLine("chessBoard.bottom_user.chess_board_empty = " + reply.ChessBoard.BottomUser.ChessBoardEmpty);
            }

            return ret;
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

                default:
                    break;
            }

            return ret;
        }
        public void ParseAnnounceMoveReply(byte [] msg)
        {
            MoveAction reply = MoveAction.ParseFrom(msg);
            Console.WriteLine("src_user_locat :" + reply.SrcUserLocate);
            Console.WriteLine("TokenLocate :" + reply.TokenLocate);
            Console.WriteLine("SrcChessType :" + reply.Movechess.SrcChessType);
            Console.WriteLine("SrcUserLocate :" + reply.Movechess.SrcUserLocate);
            Console.WriteLine("IsWinner :" + reply.Movechess.IsWinner);
            Console.WriteLine("FromPointX :" + reply.Movechess.FromPointX);
            Console.WriteLine("FromPointY :" + reply.Movechess.FromPointY);
            Console.WriteLine("DesPointX :" + reply.Movechess.DesPointX);
            Console.WriteLine("DesPointY :" + reply.Movechess.DesPointY);
        }

        public bool MoveChessReq()
        {
            MoveChess moveChess;
            MoveChess.Builder builder = new MoveChess.Builder();
            Console.WriteLine("Input Chess type:");
            builder.SetSrcChessType(Convert.ToInt32(Console.ReadLine()));
            builder.SetSrcUserLocate(locate);
            builder.SetFromPointX(100);
            builder.SetFromPointY(100);
            builder.SetDesPointX(200);
            builder.SetDesPointY(200);
            builder.SetIsWinner(false);

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
        }

        public bool SendIMMessage()
        {
            UserMessage msg;
            UserMessage.Builder builder = new UserMessage.Builder();
            Console.WriteLine("Input IM Message:");
            builder.SetMsgContent(Console.ReadLine());
            builder.SetSrcUserLocate((uint)locate);

            msg = builder.Build();
            byte[] bytes = msg.ToByteArray();

            NetworkThread.SendMessage(MessageType.MSG_USER_MSG, bytes);

            return true;
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

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

namespace WpfApplication2
{
    class NetworkThread
    {
        static private Socket tcpClient = null;
        static private int port = 8888;
        //static private string host = "123.57.180.67";
        static private string host = "10.193.90.79";
        private const int HEAD_SIZE = 2 * sizeof(uint);
        static private GameState state = null;
        private const int MAX_RECONNECT_NUM = 10;
        static private int currentReconNum = 0;

        static private Thread connectThread = null;

        static public int BytesCopy(byte[] des, byte[] src, uint size, int fromIndex = 0, int desIndex = 0)
        {
            int count = 0;
            for (int i = fromIndex; (i < src.Length) && (count++ < size); ++i)
            {
                des[desIndex++] = src[i];
            }

            return count;
        }

        static public void CreateWorkThread()
        {
            if (connectThread == null)
            {
                // 通过一个线程发起请求,多线程  
                connectThread = new Thread(new ThreadStart(NetworkThread.ThreadProc));
                //Thread connectThread = new Thread(NetworkThread.ThreadProc);
                connectThread.Start();
            }
        }
        static public void DestroryWorkThread()
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
            }
            if (connectThread != null)
            {
                connectThread.Abort();
                connectThread = null;
            }
        }

        static private bool ConnectToServer()
        {
            ///创建终结点EndPoint            
            string tmp_host = IniFileHand.ReadIniData("Server", "IP", host, GameState.gWorkPath + @"\res\files\info.ini");
            if (tmp_host != String.Empty)
            {
                host = tmp_host;
                Console.WriteLine("Got the config IP:" + host);
            }
            string tmp_port = IniFileHand.ReadIniData("Server", "Port", port.ToString(), GameState.gWorkPath + @"\res\files\info.ini");
            if (tmp_port != String.Empty)
            {
                port = int.Parse(tmp_port);
                Console.WriteLine("Got the config Port:" + tmp_port);
            }

            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, port);//把ip和端口转化为IPEndpoint实例

            try
            {
                ///创建socket并连接到服务器
                tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建Socket
                Console.WriteLine("Conneting server....");
                tcpClient.Connect(ipe);//连接到服务器
                tcpClient.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                tcpClient.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
                ++currentReconNum;
            }
            catch (Exception e)
            {
                Console.WriteLine("Connect Server {0} failed since {1}", host, e.Message);
                return false;
            }

            return true;
        }

        static private bool ReConnectServer()
        {
            if (currentReconNum < MAX_RECONNECT_NUM)
            {
                try
                {
                    tcpClient.Close();
                    Thread.Sleep(3000);
                    ConnectToServer();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Re connec server failed for " + e.Message);
                }
            }
            else
            {
                Console.WriteLine("Out of max reconnect num, exit.");
            }

            return false;
        }

        // 连接服务器方法,建立连接的过程  
        static public void ThreadProc()
        {
            ConnectToServer();

            byte[] recvBytes = new byte[1024 + 128];
            state = new LoginState();

            while (true)
            {
                int bytes = 0;
                int total_size = HEAD_SIZE;
                int has_recv = 0;
                byte[] left_msg;
                bool gotHead = false;
                int msg_type = 0;

                while (has_recv < total_size)
                {
                    try
                    {
                        bytes = tcpClient.Receive(recvBytes, has_recv, total_size - has_recv, 0);//从服务器端接受
                        if (bytes > 0)
                        {
                            has_recv += bytes;
                            Console.WriteLine("current receive {0} bytes", bytes);

                            if (!gotHead)
                            {
                                if (has_recv >= HEAD_SIZE)
                                {
                                    total_size = System.BitConverter.ToInt32(recvBytes, 0);
                                    msg_type = System.BitConverter.ToInt32(recvBytes, 4);
                                    Console.WriteLine("Got Message size {0} type {1}", total_size, msg_type);

                                    if ((total_size > recvBytes.Length) && (total_size <= 1024*1024*2))
                                    {
                                        left_msg = new byte[total_size];
                                        BytesCopy(left_msg, recvBytes, (uint)has_recv);
                                        recvBytes = left_msg;
                                    }
                                    else if ((total_size > 1024 * 1024 * 2) || (total_size < 0) || (msg_type > (int) MessageType.MSG_TYPE_MAX))
                                    {
                                        Console.WriteLine("Receive Invalid data!!!!");
                                        continue;
                                    }
                                    gotHead = true;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Receive failed. " + bytes);
                            //if (!ReConnectServer())
                                return;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Receive failed for {0}", e.Message);
                        if (!ReConnectServer())
                            return;
                    }
                }

                byte[] msg_buf = new byte[total_size - HEAD_SIZE];
                BytesCopy(msg_buf, recvBytes, (uint)msg_buf.Length, HEAD_SIZE);
                state.MessageHandle((MessageType)msg_type, msg_buf);
            }
        }

        static public void SendMessage(MessageType msg_type, byte[] buffer, bool reTry = true)
        {
            int total_size = buffer.Length + HEAD_SIZE;
            byte[] senbuf = new byte[total_size];

            byte[] size_buf = System.BitConverter.GetBytes(total_size);
            byte[] type_buf = System.BitConverter.GetBytes((int)msg_type);

            BytesCopy(senbuf, size_buf, HEAD_SIZE / 2);
            BytesCopy(senbuf, type_buf, HEAD_SIZE / 2, 0, HEAD_SIZE / 2);
            BytesCopy(senbuf, buffer, (uint)buffer.Length, 0, HEAD_SIZE);

            if (tcpClient != null)
            {
                try
                {
                    int send_bytes = 0;
                    int has_send_bytes = 0;
                    while (has_send_bytes < total_size)
                    {
                        send_bytes = tcpClient.Send(senbuf, has_send_bytes, senbuf.Length - has_send_bytes, 0);
                        if (send_bytes > 0)
                        {
                            has_send_bytes += send_bytes;
                        }
                        else
                        {
                            Console.WriteLine("Send failed. send_bytes=" + total_size);
                            if (reTry)
                            {
                                //if (!ReConnectServer())
                                   // return;
                            }
                            break;
                        }
                    }
                    Console.WriteLine("Send bytes = " + send_bytes);
                }
                catch (Exception e)
                {
                    Console.WriteLine("send failed for {0}", e.Message);
                    if (reTry)
                    {
                        //if (!ReConnectServer())
                       //     return;
                    }
                }
            }
        } 
        
        static public void SetGameState(GameState nextState)
        {
            state = nextState;
        }
    }
}

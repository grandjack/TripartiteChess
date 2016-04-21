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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum loggonState
        {
            Loggin = 0,
            Register = 1,
            FindPwd = 2,
        }
        private loggonState curState = loggonState.Loggin;
        private int actual_pwd_len = 0;
        private string record_pwd_md5 = "";
        private bool need_record_pwd = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Activate();
            
            BitmapImage bt = new BitmapImage();
            bt.BeginInit();
            bt.UriSource = new Uri(GameState.gWorkPath + @"res\Images\MyIcon.png", UriKind.Absolute);
            bt.EndInit();
            this.Icon = bt;

            string tmp_account = IniFileHand.ReadIniData("User", "Account", "None", GameState.gWorkPath + @"\res\files\info.ini");
            if ((tmp_account != String.Empty) && (!tmp_account.Equals("None")) && (tmp_account.Length > 0))
            {
                this.cbUser.Text = tmp_account;
                Console.WriteLine("Got the config account:" + tmp_account);
            }

            string tmp = IniFileHand.ReadIniData("User", "RecordPwdEn", "0", GameState.gWorkPath + @"\res\files\info.ini");
            if ((tmp != String.Empty) && (!tmp_account.Equals("0")) && (tmp_account.Length > 0))
            {
                if (int.Parse(tmp) == 1)
                {
                    record_pwd.IsChecked = true;
                    need_record_pwd = true;

                    tmp = IniFileHand.ReadIniData("User", "ActualPwdLen", "0", GameState.gWorkPath + @"\res\files\info.ini");
                    if ((tmp != String.Empty) && (!tmp.Equals("0")))
                    {
                        actual_pwd_len = int.Parse(tmp);
                        tmp = IniFileHand.ReadIniData("User", "Pwdmd5", "None", GameState.gWorkPath + @"\res\files\info.ini");
                        if ((tmp != String.Empty) && (!tmp.Equals("None")) && (tmp.Length > 0))
                        {
                            record_pwd_md5 = tmp;
                            passwordBox.Password = new String('$', actual_pwd_len);
                            Console.WriteLine("Got config pwd:" + record_pwd_md5 + " display pwd:" + passwordBox.Password);
                        }
                    }
                }
            }

            NetworkThread.CreateWorkThread();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Close_Window(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Min_Window(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        //交替显示lable还是文本框
        private void textChanged(object sender, EventArgs e)
        {
            if (sender.Equals(cbUser))
            {
                if (cbUser.Text.Length == 0)
                {
                    labelUser.Visibility = Visibility.Visible;
                }
                else
                {
                    labelUser.Visibility = Visibility.Hidden;
                }
            }
            else if (sender.Equals(passwordBox))
            {
                if (passwordBox.Password.Length == 0)
                {
                    labelPwd.Visibility = Visibility.Visible;
                }
                else
                {
                    labelPwd.Visibility = Visibility.Hidden;
                }
            }
        }

        private void label_Click(object sender, EventArgs e)
        {
            if (sender.Equals(labelUser))
            {
                labelUser.Visibility = Visibility.Hidden;
                cbUser.Focus();
                if (passwordBox.Password.Length == 0)
                {
                    labelPwd.Visibility = Visibility.Visible;
                }
                else
                {
                    labelPwd.Visibility = Visibility.Hidden;
                }
            }
            else if (sender.Equals(labelPwd))
            {
                labelPwd.Visibility = Visibility.Hidden;
                passwordBox.Focus();
                if (cbUser.Text.Length == 0)
                {
                    labelUser.Visibility = Visibility.Visible;
                }
                else
                {
                    labelUser.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Register(object sender, EventArgs e)
        {
            register.Visibility = Visibility.Hidden; 
            forget.Visibility = Visibility.Hidden;
            cancelBtn.Visibility = Visibility.Visible;
            record_pwd.Visibility = System.Windows.Visibility.Hidden;

            logginBtn.Content = (string)Properties.Resources.register;
            labelUser.Content = (string)Properties.Resources.email_input;
            cbUser.Text = "";
            passwordBox.Password = "";
            curState = loggonState.Register;
        }

        private void FindPwd(object sender, EventArgs e)
        {
            register.Visibility = Visibility.Hidden;
            forget.Visibility = Visibility.Hidden;
            cancelBtn.Visibility = Visibility.Visible;
            labelPwd.Visibility = Visibility.Hidden;
            passwordBox.Visibility = Visibility.Hidden;
            record_pwd.Visibility = System.Windows.Visibility.Hidden;

            labelUser.Content = (string)Properties.Resources.email_input;
            logginBtn.Content = (string)Properties.Resources.find_pwd; 
            cbUser.Text = "";

            curState = loggonState.FindPwd;
        }

        public void CancelBtn(object sender, RoutedEventArgs e)
        {
            if (curState == loggonState.FindPwd)
            {
                passwordBox.Visibility = Visibility.Visible;
            }

            record_pwd.Visibility = System.Windows.Visibility.Visible;
            register.Visibility = Visibility.Visible;
            forget.Visibility = Visibility.Visible;
            logginBtn.Visibility = System.Windows.Visibility.Visible;
            labelUser.Visibility = System.Windows.Visibility.Visible;
            labelPwd.Visibility = System.Windows.Visibility.Visible;

            logginBtn.Content = (string)Properties.Resources.Login_name;
            labelUser.Content = (string)Properties.Resources.account_info;
            labelPwd.Content = (string)Properties.Resources.paswd_info;

            cancelBtn.Visibility = Visibility.Hidden;
            cbUser.Text = "";
            passwordBox.Password = "";

            curState = loggonState.Loggin;
        }


        public Loading ld = null;
        public void LoadingImage()
        {
            ld = new Loading();
            ld.Owner = this;
            //ld.Owner.Opacity = 0.5;
            ld.IniLocate();
            ld.Show();
        }
        public void LoadingImageClose()
        {
            //ld.Owner.Opacity = 1;
            ld.Close();
        }

        public int click_count = 0;
        private void LogginGame(object sender, RoutedEventArgs e)
        {
            if (click_count > 0)
            {
                return;
            }

            if (curState == loggonState.Loggin)
            {
                //verify the User's account whether match the password,if OK, then show the MainWindow
                if (cbUser.Text.Length == 0)
                {
                    WindowShowTimer box = new WindowShowTimer(this, "错误", "账号不能为空", 2);
                    box.Show();
                    return;
                }
                if (passwordBox.Password.Length == 0)
                {
                    WindowShowTimer box = new WindowShowTimer(this, "错误", "密码不能为空", 2);
                    box.Show();
                    return;
                }
                if ((cbUser.Text.IndexOf("@") < 0) || (cbUser.Text.IndexOf(".") < 0)
                    || (cbUser.Text.IndexOf("@") >= cbUser.Text.Length - 3)
                    || (cbUser.Text.IndexOf("@") > cbUser.Text.IndexOf(".")))
                {
                    WindowShowTimer box = new WindowShowTimer(this, "错误", "请输入有效的邮箱账号", 2);
                    box.Show();
                    return;
                }

                ++click_count;

                //NetworkThread.CreateWorkThread();
                GameState.SetCurrentWin(this);
                GameState.SetLogWin(this);

                //加载中。。。。。。
                LoadingImage();

                bool use_record = false;
                if ((actual_pwd_len > 0) && 
                    (record_pwd_md5.Length == 32))
                {
                    string tmp_pwd = new String('$', actual_pwd_len);
                    if (passwordBox.Password.Equals(tmp_pwd))
                    {
                        LoginState login = new LoginState();
                        login.SendLoginReq(cbUser.Text, record_pwd_md5);
                        use_record = true;
                    }
                }

                if (!use_record)
                {
                    actual_pwd_len = passwordBox.Password.Length;
                    MD5Hash hash = new MD5Hash(passwordBox.Password);
                    LoginState login = new LoginState();
                    login.SendLoginReq(cbUser.Text, hash.GetMD5HashCode());
                }

                GameState.currentUserActualPassword = passwordBox.Password;
            }
            else if (curState == loggonState.Register)
            {
                if (cbUser.Text.Length == 0)
                {
                    WindowShowTimer box = new WindowShowTimer(this, "错误", "邮箱地址不能为空", 2);
                    box.Show();
                    return;
                }

                if (passwordBox.Password.Length == 0)
                {
                    WindowShowTimer box = new WindowShowTimer(this, "错误", "密码不能为空", 2);
                    box.Show();
                    return;
                }

                if ((cbUser.Text.IndexOf("@") < 0) || (cbUser.Text.IndexOf(".") < 0)
                    || (cbUser.Text.IndexOf("@") >= cbUser.Text.Length - 3)
                    || (cbUser.Text.IndexOf("@") > cbUser.Text.IndexOf(".")))
                {
                    WindowShowTimer box = new WindowShowTimer(this, "错误", "请输入有效的邮箱地址", 2);
                    box.Show();
                    return;
                }

                ++click_count;

                GameState.SetCurrentWin(this);
                GameState.SetLogWin(this);

                MD5Hash hash = new MD5Hash(passwordBox.Password);
                LoginState register = new LoginState();
                register.SendRegist(cbUser.Text, hash.GetMD5HashCode());
            } else if (curState == loggonState.FindPwd)
            {
                if ((cbUser.Text.IndexOf("@") < 0) || (cbUser.Text.IndexOf("@") >= cbUser.Text.Length -3))
                {
                    WindowShowTimer box = new WindowShowTimer(this, "错误", "请输入有效的邮箱地址", 2);
                    box.Show();
                    return;
                }

                ++click_count;
                GameState.SetCurrentWin(this);
                GameState.SetLogWin(this);
                LoginState state = new LoginState();
                state.FindUsrPassword(cbUser.Text);

                WindowShowTimer box2 = new WindowShowTimer(this, "提示", "已向您发送邮件，请在5分钟内登录修改密码", 60);
                box2.Show();
            }
            else
            {
                //Error
            }
        }

        private void ClosingLogWin(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NetworkThread.DestroryWorkThread();
        }

        private void LogBorderLoad(object sender, RoutedEventArgs e)
        {
            BitmapImage bt = new BitmapImage();
            bt.BeginInit();

            bt.UriSource = new Uri(GameState.gWorkPath + @"res\Images\logon\logon.jpg", UriKind.Absolute);
            bt.EndInit();

            loggin_border.Background = new ImageBrush(bt);
        }

        private void RecordClick(object sender, RoutedEventArgs e)
        {
            if (record_pwd.IsChecked == true)
            {
                need_record_pwd = true;
            }
            else
            {
                need_record_pwd = false;
            }
        }

        public void SavePassword()
        {
            if (need_record_pwd)
            {
                IniFileHand.WriteIniData("User", "RecordPwdEn", "1", GameState.gWorkPath + @"\res\files\info.ini");
                IniFileHand.WriteIniData("User", "ActualPwdLen", actual_pwd_len.ToString(), GameState.gWorkPath + @"\res\files\info.ini");
                IniFileHand.WriteIniData("User", "Pwdmd5", GameState.currentUserPassword, GameState.gWorkPath + @"\res\files\info.ini");
            }
            else
            {
                IniFileHand.WriteIniData("User", "RecordPwdEn", "0", GameState.gWorkPath + @"\res\files\info.ini");
                IniFileHand.WriteIniData("User", "ActualPwdLen", "0", GameState.gWorkPath + @"\res\files\info.ini");
                IniFileHand.WriteIniData("User", "Pwdmd5", "None", GameState.gWorkPath + @"\res\files\info.ini");
            }

            GameState.currentUserActualPassword = passwordBox.Password;
        }

        public bool NeedRecordPwd()
        {
            return this.need_record_pwd;
        }
    }
}

﻿using System;
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

        public MainWindow()
        {
            InitializeComponent();
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

            labelUser.Content = (string)Properties.Resources.email_input;
            logginBtn.Content = (string)Properties.Resources.find_pwd; 
            cbUser.Text = "";

            curState = loggonState.FindPwd;
        }

        private void CancelBtn(object sender, RoutedEventArgs e)
        {
            if (curState == loggonState.FindPwd)
            {
                passwordBox.Visibility = Visibility.Visible;
            }
            register.Visibility = Visibility.Visible;
            forget.Visibility = Visibility.Visible;
            logginBtn.Content = (string)Properties.Resources.Login_name;
            labelUser.Content = (string)Properties.Resources.account_info;
            cancelBtn.Visibility = Visibility.Hidden;
            cbUser.Text = "";
            passwordBox.Password = "";

            curState = loggonState.Loggin;
        }

        private void LogginGame(object sender, RoutedEventArgs e)
        {
            if (curState == loggonState.Loggin)
            {
                //verify the User's account whether match the password,if OK, then show the MainWindow
                if (cbUser.Text.Length == 0)
                {
                    MessageBox.Show("用户名不能为空");
                    return;
                }
                if (passwordBox.Password.Length == 0)
                {
                    MessageBox.Show("密码不能为空");
                    return;
                }
                Window1 gameHall = new Window1();
                this.Hide();
                gameHall.ShowDialog();
                this.Close();

            } else if (curState == loggonState.Register) {
                MessageBox.Show("暂不支持注册功能");
            } else if (curState == loggonState.FindPwd)
            {
                MessageBox.Show("暂不支持找回密码功能");
            }
            else
            {
                //Error
            }
        }

    }
}

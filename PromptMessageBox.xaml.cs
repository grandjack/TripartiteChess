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
using System.Windows.Shapes;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for PromptMessageBox.xaml
    /// </summary>
    public partial class PromptMessageBox : Window
    {
        public PromptMessageBox(string title, string msg)
        {
            InitializeComponent();
            this.title_lab.Content = title;
            this.content_tx.Text = msg;
        }

        private void Close_Window(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }

    public class WindowShowTimer
    {
        private PromptMessageBox win = null;
        private Window own_win = null;
        private int time = 5;//seconds
        private string title = null;
        private string message = null;

        System.Windows.Threading.DispatcherTimer dispatcherTimer = null;

        public WindowShowTimer(Window own_win, string title, string message, int time)
        {
            this.time = time;
            this.title = title;
            this.message = message;
            this.own_win = own_win;

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            if (this.time == -1)
            {
                this.time = 60 * 60;
            }

            dispatcherTimer.Interval = new TimeSpan(0, 0, this.time);
        }
        
        public WindowShowTimer(Window own_win, string title, string message)
        {
            this.title = title;
            this.message = message;
            this.own_win = own_win;

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, this.time);
        }

        public void Show()
        {
            this.Start();
            win = new PromptMessageBox(title, message);
            win.Owner = own_win;
            win.Show();
        }


        public void Start()
        {
            dispatcherTimer.Start();
        }

        public void Stop()
        {
            dispatcherTimer.Stop();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            win.Close();
            CommandManager.InvalidateRequerySuggested();
            return;
        }
    }
}

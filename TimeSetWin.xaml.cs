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
    /// Interaction logic for TimeSetWin.xaml
    /// </summary>
    public partial class TimeSetWin : Window
    {
        public TimeSetWin()
        {
            InitializeComponent();
        }

        private void Close_Window(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public int total_sec = 2*60*60;
        public int step_sec = 2*60;
        private void Click_OK_Btn(object sender, RoutedEventArgs e)
        {
            int total_hour = 1;
            int total_min = 0;
            int step_min = 2;
            int step_sec = 0;
            try
            {
                total_hour = int.Parse(comboBoxTotalHour.Text.ToString());
                total_min = int.Parse(comboBoxTotalMin.Text.ToString());
                step_min = int.Parse(comboBoxStepMin.Text.ToString());
                step_sec = int.Parse(comboBoxStepSec.Text.ToString());
            }
            catch
            {
                Console.WriteLine("Convert failed.");
            }

            if (((total_hour == 0) && (total_min == 0)) ||
                ((step_min == 0) && (step_sec == 0)))
            {
                MessageBox.Show("不能同时设置为0", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.total_sec = (total_hour*60 + total_min)*60;
            this.step_sec = step_min * 60 + step_sec;

            Console.WriteLine("total_sec " + total_sec + " step_sec" + this.step_sec);

            GamePlayingState state = new GamePlayingState();
            state.StartGameReadyReq(this.total_sec, this.step_sec);

            this.Close();
        }

        private void Click_Cancel_Btn(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

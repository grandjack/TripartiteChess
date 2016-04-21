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
    /// Interaction logic for MyMessageBox.xaml
    /// </summary>
    public partial class MyMessageBox : Window
    {
        private static MessageBoxResult result = MessageBoxResult.Cancel;
        private static bool isModalDialogue = true;
        private MessageBoxButton button;

        public MyMessageBox(string title, string content, MessageBoxButton button)
        {
            InitializeComponent();
            title_lab.Content = title;
            content_lab.Text = content;
            this.button = button;

            if (button == MessageBoxButton.OKCancel)
            {
                okBtn.Content = (string)Properties.Resources.btn_ok;
                cancelBtn.Content = (string)Properties.Resources.cancelBtn;
            }
            else
            {
                okBtn.Content = (string)Properties.Resources.game_again;
                cancelBtn.Content = (string)Properties.Resources.exit;
            }
        }

        private void Close_Window(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void OK_click(object sender, RoutedEventArgs e)
        {
            try
            {
                GameState.again_play = true;
                if (GameState.gameWin != null)
                    GameState.gameWin.pressGameReadyBtn = false;

                if (!MyMessageBox.isModalDialogue)
                {
                    if (GameState.gameWin != null)
                    {
                        GameReadyState state = new GameReadyState();
                        state.RequestGamePlay(GameState.gameHallID, GameState.chessBoardID, GameState.locate);
                        GameState.gameWin.EndGame();
                    }
                }

                if (GameState.gameWin != null)
                    GameState.gameWin.DisplayUserStatus((uint)GameState.locate, UserStatus.STATUS_READY);

                this.Close();

                if (button == MessageBoxButton.YesNo)
                {
                    result = MessageBoxResult.Yes;
                }
                else
                {
                    result = MessageBoxResult.OK;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("Catch expection : " + ee.Message);
            }
        }

        private void No_click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GameState.again_play)
                {
                    GameState.again_play = false;
                    if (GameState.gameWin != null)
                        GameState.gameWin.SetStartButtonStatus(false);
                }

                //对应模态对话框，必须先关闭当前窗口，之后才能关闭父窗口
                this.Close();

                if (button == MessageBoxButton.YesNo)
                {
                    if (GameState.gameWin != null)
                        GameState.gameWin.Close();

                    result = MessageBoxResult.No;
                }
                else
                {
                    result = MessageBoxResult.Cancel;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine("Catch expection : " + ee.Message);
            }
        }

        public static MessageBoxResult Show(Window owner, string content, string title, MessageBoxButton button, bool isModalDialogue=true)
        {
            MyMessageBox box = new MyMessageBox(title, content, button);
            box.Owner = owner;

            MyMessageBox.isModalDialogue = isModalDialogue;

            if (isModalDialogue)
            {
                box.ShowDialog();
            }
            else
            {
                box.Show();
            }

            return result;
        }
    }
}

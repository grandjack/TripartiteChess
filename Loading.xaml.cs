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
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Window
    {
        public Loading()
        {
            InitializeComponent();
            AnimatedGIFControl gif = new AnimatedGIFControl(GameState.gWorkPath + @"\res\Images\loading.gif");
            gif.Cursor = Cursors.Hand;
            gif.StartAnimate();
            //gif.Opacity = 0.7;
            gif.Stretch = Stretch.Uniform;
            gif.Height = 100;
            mainGrid.Children.Add(gif);
        }

        public void IniLocate()
        {
            if (this.Owner != null)
            {
                this.Left = this.Owner.Left;
                this.Top = this.Owner.Top;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PermDynamics.View
{
    /// <summary>
    /// Interaction logic for BuySellShareWindow.xaml
    /// </summary>
    public partial class BuySellShareWindow : Window
    {
        public BuySellShareWindow()
        {
            InitializeComponent();
            txt_count.Focus();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

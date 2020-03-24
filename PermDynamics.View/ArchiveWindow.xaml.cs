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
using PermDynamics.Model;

namespace PermDynamics.View
{
    /// <summary>
    /// Interaction logic for ArchiveWindow.xaml
    /// </summary>
    public partial class ArchiveWindow : Window
    {
        public ArchiveWindow()
        {
            InitializeComponent();
        }

        private void SetOperations()
        {
            var users = SharesModel.GetUsers();
            var operation = SharesModel.GetOperations();
            var operations = operation.Select(x => new
            {
                Type = x.IsBuying ? "Покупка" : "Продажа", Count = x.Count, Price = (double)Math.Round(x.Price, 2),
                User = users.First(u => u.Id == x.UserId).Name
            });
            datagrid_operations.ItemsSource = operations;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            SetOperations();
        }
    }
}

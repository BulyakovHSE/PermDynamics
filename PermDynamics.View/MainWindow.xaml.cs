using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PermDynamics.Model;
using PermDynamics.View.Model;
using PermDynamics.View.ViewModels;

namespace PermDynamics.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            InitDb();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Все графики находятся в пределах области построения ChartArea, создадим ее
            chart.ChartAreas.Add(new ChartArea("Default"));

            // Добавим линию, и назначим ее в ранее созданную область "Default"
            chart.Series.Add(new Series("Perm Dynamics"));
            chart.Series["Perm Dynamics"].ChartArea = "Default";
            chart.Series["Perm Dynamics"].ChartType = SeriesChartType.Line;
            chart.Series["Perm Dynamics"].AxisLabel = "Perm Dynamics";

            var r = new Random();
            decimal last = 10;
            var c = SynchronizationContext.Current;
            var datetime = DateTime.Now.AddSeconds(-200);
            for (int i = 0; i < 199; i++)
            {
                chart.Series["Perm Dynamics"].Points.AddXY(datetime.AddSeconds(i).ToLongTimeString(), last);
                last += (decimal)r.NextDouble() - 0.45m;
            }
            var task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    decimal delta = 0;
                    do
                    {
                        delta = (decimal)r.NextDouble() - 0.47m;
                    } while (last < 0.1m && delta < 0);

                    last += delta;

                    c.Send(state =>
                    {
                        chart.Series["Perm Dynamics"].Points.AddXY(DateTime.Now.ToLongTimeString(), last);
                        if (chart.Series["Perm Dynamics"].Points.Count == 200)
                            chart.Series["Perm Dynamics"].Points.RemoveAt(0);
                        chart.ResetAutoValues();
                    }, null);


                    _viewModel.CurrentPrice = last;

                    Thread.Sleep(1000);
                }
            });
        }

        private void InitDb()
        {
            //SharesModel.DeleteTables();
            SharesModel.CreateTables();
            var users = SharesModel.GetUsers();
            if (users.Count == 0)
            {
                var user = new User() { Id = 0, Name = "Пользователь", Money = 400000, ShareCount = 0 };
                var virtualUser = new User() { Id = 1, Name = "Виртуальный брокер", Money = 400000, ShareCount = 0 };
                SharesModel.AddUser(user);
                SharesModel.AddUser(virtualUser);
            }

            if (users.Count == 1)
            {
                var user = new User() { Id = 0, Name = "Пользователь", Money = 400000, ShareCount = 0 };
                var virtualUser = new User() { Id = 1, Name = "Виртуальный брокер", Money = 400000, ShareCount = 0 };

                if (users.First().Name == "Пользователь")
                {
                    SharesModel.AddUser(virtualUser);
                }
                else if (users.First().Name == "Виртуальный брокер")
                {
                    SharesModel.AddUser(user);
                }
                else
                {
                    SharesModel.AddUser(user);
                    SharesModel.AddUser(virtualUser);
                }
            }
        }
    }
}

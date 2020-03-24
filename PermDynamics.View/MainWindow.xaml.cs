using System;
using System.Collections.Generic;
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

            //// добавим данные линии
            //string[] axisXData = new string[] { "a", "b", "c" };
            //double[] axisYData = new double[] { 0.1, 1.5, 1.9 };
            //chart.Series["Perm Dynamics"].Points.DataBindXY(axisXData, axisYData);
            var r = new Random();
            decimal last = 10;
            var c = SynchronizationContext.Current;
            for (int i = 0; i < 299; i++)
            {
                chart.Series["Perm Dynamics"].Points.AddXY(DateTime.Now.ToLongTimeString(), last);
                last += (decimal)r.NextDouble() - 0.45m;
            }
            var task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    decimal delta = 0;
                    do
                    {
                        delta = (decimal) r.NextDouble() - 0.5m;
                    } while (last < 0.1m && delta < 0);

                    last += delta;

                    c.Send(state =>
                    {
                        chart.Series["Perm Dynamics"].Points.AddXY(DateTime.Now.ToLongTimeString(), last);
                        if (chart.Series["Perm Dynamics"].Points.Count == 300)
                            chart.Series["Perm Dynamics"].Points.RemoveAt(0);
                    }, null);
                    

                    _viewModel.CurrentPrice = last;

                    Thread.Sleep(1000);
                }
            });
            
        }
    }
}

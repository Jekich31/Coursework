using System.Linq;
using System.Windows;
using ProjectFIN.models;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }
            InitializeComponent();
            DataContext = new MainViewModel();
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                bool hasUnsafeVehicles = vm.Vehicles.Any(v =>
                    v.Engine == EngineState.Running ||
                    v.Doors == DoorState.Unlocked
                );

                if (hasUnsafeVehicles)
                {
                    string title = Application.Current.Resources["msg_ConfirmExitTitle"] as string
                                   ?? "Увага!";
                    string message = Application.Current.Resources["msg_ConfirmExitWarning"] as string
                                     ?? "У вас залишилися автомобілі з працюючим двигуном або відкритими дверима! Ви впевнені, що хочете вийти?";

                    var result = MessageBox.Show(
                        message,
                        title,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning
                    );

                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }
    }
}
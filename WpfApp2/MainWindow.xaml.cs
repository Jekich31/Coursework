using ProjectFIN.models;
using System.Windows;

namespace ProjectFIN.UI
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
        }
    }
}
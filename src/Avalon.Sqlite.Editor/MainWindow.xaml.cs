using System.Windows;

namespace Avalon.Sqlite.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await sqlite.OpenDb(@"C:\Users\blake\desktop\dsl.db");
            await sqlite.RefreshSchemaAsync();
            sqlite.ExpandTableNode();
        }
    }
}

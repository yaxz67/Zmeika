using Avalonia.Controls;

namespace Zmeika.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            Content = new MainMenuView();
        }
    }
}
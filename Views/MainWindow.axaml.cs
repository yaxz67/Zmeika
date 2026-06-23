using Avalonia.Controls;
using Avalonia.Input;
using Zmeika.ViewModels;
using System.Diagnostics;

namespace Zmeika.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Content = new MainMenuView();
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine($"Клавиша нажата в MainWindow: {e.Key}");
            
            if (e.Key == Key.Escape)
            {
                Debug.WriteLine("ESC обнаружен в MainWindow!");
                
                if (Content is GameView gameView)
                {
                    var vm = gameView.DataContext as GameViewModel;
                    vm?.Cleanup();
                    
                    Content = new MainMenuView();
                    e.Handled = true;
                }
            }
        }
    }
}
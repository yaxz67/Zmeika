using Avalonia.Controls;
using Zmeika.Models;
using Zmeika.ViewModels;

namespace Zmeika.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            var menuView = new MainMenuView();
            menuView.StartGameRequested += OnStartGameRequested;
            
            MainContent.Content = menuView;
        }

        private void OnStartGameRequested(object sender, GameSettings settings)
        {
            var gameVm = new GameViewModel(settings);
            var gameView = new GameView { DataContext = gameVm };
            
            MainContent.Content = gameView;
            gameView.Focus();
        }
    }
}
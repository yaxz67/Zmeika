using Avalonia.Controls;
using Avalonia.Interactivity;
using Zmeika.Models;
using Zmeika.ViewModels;
using System;
using System.Linq;

namespace Zmeika.Views
{
    public partial class MainMenuView : UserControl
    {
        public event EventHandler<GameSettings> StartGameRequested;

        public MainMenuView()
        {
            InitializeComponent();
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            var settings = new GameSettings
            {
                EnableTeleport = TeleportCheckBox.IsChecked ?? true,
                RgbSnake = RgbCheckBox.IsChecked ?? false,
                GameSpeed = 150
            };

            StartGameRequested?.Invoke(this, settings);
            
            if (StartGameRequested == null || !StartGameRequested.GetInvocationList().Any())
            {
                var parent = this.Parent;
                while (parent != null && !(parent is MainWindow))
                {
                    parent = parent.Parent;
                }
                
                if (parent is MainWindow mainWindow)
                {
                    var gameVm = new GameViewModel(settings);
                    var gameView = new GameView { DataContext = gameVm };
                    
                    var contentControl = mainWindow.FindControl<ContentControl>("MainContent");
                    if (contentControl != null)
                    {
                        contentControl.Content = gameView;
                        gameView.Focus();
                    }
                    else
                    {
                        mainWindow.Content = gameView;
                        gameView.Focus();
                    }
                }
            }
        }
    }
}
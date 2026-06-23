using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Zmeika.Models;
using Zmeika.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;

namespace Zmeika.Views
{
    public partial class MainMenuView : UserControl
    {
        public MainMenuView()
        {
            InitializeComponent();
            
            PlayButton.Click += StartGame_Click;
            
            // Обновляем лейбл при изменении слайдера
            RgbSpeedSlider.PropertyChanged += (s, e) =>
            {
                if (e.Property.Name == "Value")
                {
                    RgbSpeedLabel.Text = $"{RgbSpeedSlider.Value:F1}x";
                }
            };
            
            this.AttachedToVisualTree += (s, e) =>
            {
                Focus();
                PlayButton.Focus();
            };
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            var settings = new GameSettings
            {
                EnableTeleport = TeleportCheckBox.IsChecked ?? true,
                RgbSnake = RgbCheckBox.IsChecked ?? false,
                GameSpeed = 150,
                RgbSpeed = RgbSpeedSlider.Value // Передаём скорость RGB
            };

            var gameVm = new GameViewModel(settings);
            
            var mainWindow = this.GetVisualAncestors()
                .OfType<MainWindow>()
                .FirstOrDefault();
                
            if (mainWindow != null)
            {
                var windowWidth = mainWindow.ClientSize.Width;
                var windowHeight = mainWindow.ClientSize.Height;
                
                gameVm.Initialize((int)windowWidth - 4, (int)windowHeight - 4);
                
                var gameView = new GameView { DataContext = gameVm };
                
                mainWindow.Content = gameView;
                gameView.Focus();
                Debug.WriteLine($"Змейка запущена с RGB скоростью: {settings.RgbSpeed}x");
            }
        }
    }
}
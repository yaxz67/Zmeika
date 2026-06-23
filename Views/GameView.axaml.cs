using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Zmeika.Models;
using Zmeika.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Zmeika.Views
{
    public partial class GameView : UserControl
    {
        private GameViewModel _viewModel;

        public GameView()
        {
            InitializeComponent();
            Focusable = true;
            
            DataContextChanged += OnDataContextChanged;
            
            this.AttachedToVisualTree += (s, e) => 
            {
                Focus();
            };
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as GameViewModel;
            if (vm == null || vm.IsGameOver) return;

            switch (e.Key)
            {
                case Key.M:
                    vm.ToggleRadioCommand.Execute().Subscribe(_ => { });
                    e.Handled = true;
                    break;
                case Key.N:
                    vm.NextStationCommand.Execute().Subscribe(_ => { });
                    e.Handled = true;
                    break;
                case Key.W or Key.Up:
                    vm.ChangeDirectionCommand.Execute(Direction.Up).Subscribe(_ => { });
                    e.Handled = true;
                    break;
                case Key.S or Key.Down:
                    vm.ChangeDirectionCommand.Execute(Direction.Down).Subscribe(_ => { });
                    e.Handled = true;
                    break;
                case Key.A or Key.Left:
                    vm.ChangeDirectionCommand.Execute(Direction.Left).Subscribe(_ => { });
                    e.Handled = true;
                    break;
                case Key.D or Key.Right:
                    vm.ChangeDirectionCommand.Execute(Direction.Right).Subscribe(_ => { });
                    e.Handled = true;
                    break;
            }
        }

        private void OnDataContextChanged(object sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.SnakeBody.CollectionChanged -= OnSnakeChanged;
            }

            _viewModel = DataContext as GameViewModel;
            
            if (_viewModel != null)
            {
                _viewModel.SnakeBody.CollectionChanged += OnSnakeChanged;
                _viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(GameViewModel.SnakeColor))
                        DrawSnake();
                    if (args.PropertyName == nameof(GameViewModel.Food))
                        DrawFood();
                };
                
                DrawFood();
                DrawSnake();
            }
        }

        private void OnSnakeChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DrawSnake();
        }

        private void DrawSnake()
        {
            if (_viewModel?.SnakeBody == null) return;
            
            var oldSnakes = GameCanvas.Children
                .OfType<Rectangle>()
                .Where(r => r.Tag?.ToString() == "snake")
                .ToList();
            
            foreach (var rect in oldSnakes)
                GameCanvas.Children.Remove(rect);

            var color = Color.Parse(_viewModel.SnakeColor ?? "#00FF00");
            var cellSize = _viewModel.CellSize;
            var elementSize = _viewModel.ElementSize;
            
            foreach (var segment in _viewModel.SnakeBody)
            {
                var rect = new Rectangle
                {
                    Width = elementSize,
                    Height = elementSize,
                    Fill = new SolidColorBrush(color),
                    Tag = "snake",
                    RadiusX = 4,
                    RadiusY = 4
                };
                
                Canvas.SetLeft(rect, segment.X * cellSize + 1);
                Canvas.SetTop(rect, segment.Y * cellSize + 1);
                GameCanvas.Children.Add(rect);
            }
        }

        private void DrawFood()
        {
            var oldFood = GameCanvas.Children
                .OfType<Rectangle>()
                .FirstOrDefault(r => r.Tag?.ToString() == "food");
            
            if (oldFood != null)
                GameCanvas.Children.Remove(oldFood);

            if (_viewModel?.Food == null) return;

            var cellSize = _viewModel.CellSize;
            var elementSize = _viewModel.ElementSize;

            var foodRect = new Rectangle
            {
                Width = elementSize,
                Height = elementSize,
                Fill = new SolidColorBrush(Colors.Red),
                Tag = "food",
                RadiusX = elementSize / 2,
                RadiusY = elementSize / 2
            };
            
            Canvas.SetLeft(foodRect, _viewModel.Food.X * cellSize + 1);
            Canvas.SetTop(foodRect, _viewModel.Food.Y * cellSize + 1);
            GameCanvas.Children.Add(foodRect);
        }
    }
}
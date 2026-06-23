using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using ReactiveUI;
using Zmeika.Models;

namespace Zmeika.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private const int GridSize = 20;
        private readonly GameSettings _settings;
        private readonly Random _random = new Random();
        private IDisposable _gameLoop;

        private Snake _snake;
        private Position _food;
        private int _score;
        private bool _isGameOver;
        private double _hue;
        private string _snakeColor = "#00FF00";

        public int CellSize => 30;
        
        public int CanvasSize => GridSize * CellSize;
        
        public int ElementSize => CellSize - 2;

        public ObservableCollection<Position> SnakeBody => _snake?.Body;
        
        public Position Food
        {
            get => _food;
            set => this.RaiseAndSetIfChanged(ref _food, value);
        }

        public int Score
        {
            get => _score;
            set => this.RaiseAndSetIfChanged(ref _score, value);
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            set => this.RaiseAndSetIfChanged(ref _isGameOver, value);
        }

        public string SnakeColor
        {
            get => _snakeColor;
            set => this.RaiseAndSetIfChanged(ref _snakeColor, value);
        }

        public ReactiveCommand<Direction, Unit> ChangeDirectionCommand { get; }

        public GameViewModel(GameSettings settings)
        {
            _settings = settings;
            _snake = new Snake(GridSize / 2, GridSize / 2);
            
            ChangeDirectionCommand = ReactiveCommand.Create<Direction>(dir =>
            {
                if (!IsOppositeDirection(dir))
                    _snake.CurrentDirection = dir;
            });

            SpawnFood();
            StartGameLoop();
        }

        private void StartGameLoop()
        {
            _gameLoop = Observable.Interval(TimeSpan.FromMilliseconds(_settings.GameSpeed))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Update());
        }

        private void Update()
        {
            if (IsGameOver) return;

            var head = _snake.Body[0];
            var newHead = new Position { X = head.X, Y = head.Y };

            switch (_snake.CurrentDirection)
            {
                case Direction.Up: newHead.Y--; break;
                case Direction.Down: newHead.Y++; break;
                case Direction.Left: newHead.X--; break;
                case Direction.Right: newHead.X++; break;
            }

            if (_settings.EnableTeleport)
            {
                if (newHead.X < 0) newHead.X = GridSize - 1;
                if (newHead.X >= GridSize) newHead.X = 0;
                if (newHead.Y < 0) newHead.Y = GridSize - 1;
                if (newHead.Y >= GridSize) newHead.Y = 0;
            }
            else
            {
                if (newHead.X < 0 || newHead.X >= GridSize || 
                    newHead.Y < 0 || newHead.Y >= GridSize)
                {
                    GameOver();
                    return;
                }
            }

            if (_snake.Body.Any(p => p.X == newHead.X && p.Y == newHead.Y))
            {
                GameOver();
                return;
            }

            _snake.Body.Insert(0, newHead);

            if (newHead.X == Food.X && newHead.Y == Food.Y)
            {
                Score += 10;
                SpawnFood();
            }
            else
            {
                _snake.Body.RemoveAt(_snake.Body.Count - 1);
            }

            if (_settings.RgbSnake)
            {
                _hue = (_hue + 0.02) % 1.0;
                SnakeColor = HsvToRgb(_hue, 1, 1);
            }

            this.RaisePropertyChanged(nameof(SnakeBody));
        }

        private void SpawnFood()
        {
            Position newFood;
            do
            {
                newFood = new Position
                {
                    X = _random.Next(0, GridSize),
                    Y = _random.Next(0, GridSize)
                };
            } while (_snake.Body.Any(p => p.X == newFood.X && p.Y == newFood.Y));

            Food = newFood;
        }

        private bool IsOppositeDirection(Direction dir)
        {
            return (dir == Direction.Up && _snake.CurrentDirection == Direction.Down) ||
                   (dir == Direction.Down && _snake.CurrentDirection == Direction.Up) ||
                   (dir == Direction.Left && _snake.CurrentDirection == Direction.Right) ||
                   (dir == Direction.Right && _snake.CurrentDirection == Direction.Left);
        }

        private void GameOver()
        {
            IsGameOver = true;
            _gameLoop?.Dispose();
        }

        private string HsvToRgb(double h, double s, double v)
        {
            int hi = (int)(h * 6);
            double f = h * 6 - hi;
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);

            double r, g, b;
            switch (hi % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }

            return $"#{(int)(r * 255):X2}{(int)(g * 255):X2}{(int)(b * 255):X2}";
        }
    }
}
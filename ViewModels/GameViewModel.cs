using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using Zmeika.Models;
using Zmeika.Services;

namespace Zmeika.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private readonly GameSettings _settings;
        private readonly Random _random = new Random();
        private readonly RadioService _radioService = new RadioService();
        private IDisposable _gameLoop;

        private Snake _snake;
        private Position _food;
        private int _score;
        private bool _isGameOver;
        private double _hue;
        private string _snakeColor = "#00FF00";
        private string _weatherInfo = "Загрузка погоды...";
        private string _radioInfo = "Радио выключено";
        
        private int _canvasWidth = 800;
        private int _canvasHeight = 600;
        private int _gridWidth = 30;
        private int _gridHeight = 20;
        private int _cellSize = 20;

        public int CanvasWidth
        {
            get => _canvasWidth;
            set => this.RaiseAndSetIfChanged(ref _canvasWidth, value);
        }

        public int CanvasHeight
        {
            get => _canvasHeight;
            set => this.RaiseAndSetIfChanged(ref _canvasHeight, value);
        }

        public int GridWidth
        {
            get => _gridWidth;
            set => this.RaiseAndSetIfChanged(ref _gridWidth, value);
        }

        public int GridHeight
        {
            get => _gridHeight;
            set => this.RaiseAndSetIfChanged(ref _gridHeight, value);
        }

        public int CellSize
        {
            get => _cellSize;
            set => this.RaiseAndSetIfChanged(ref _cellSize, value);
        }

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

        public string WeatherInfo
        {
            get => _weatherInfo;
            set => this.RaiseAndSetIfChanged(ref _weatherInfo, value);
        }

        public string RadioInfo
        {
            get => _radioInfo;
            set => this.RaiseAndSetIfChanged(ref _radioInfo, value);
        }

        public ReactiveCommand<Direction, Unit> ChangeDirectionCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> ToggleRadioCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> NextStationCommand { get; private set; }

        public GameViewModel(GameSettings settings)
        {
            _settings = settings;
            
            ChangeDirectionCommand = ReactiveCommand.Create<Direction>(dir =>
            {
                if (_snake != null && !IsOppositeDirection(dir))
                    _snake.CurrentDirection = dir;
            });
            
            ToggleRadioCommand = ReactiveCommand.Create(() =>
            {
                if (_radioService.CurrentStation != null)
                {
                    _radioService.Stop();
                    RadioInfo = "Радио выключено";
                }
                else
                {
                    _radioService.PlayStation(0);
                    RadioInfo = $"{_radioService.CurrentStation.Emoji} {_radioService.CurrentStation.Name}";
                }
            });
            
            NextStationCommand = ReactiveCommand.Create(() =>
            {
                _radioService.NextStation();
                if (_radioService.CurrentStation != null)
                {
                    RadioInfo = $"{_radioService.CurrentStation.Emoji} {_radioService.CurrentStation.Name}";
                }
            });
        }

        public void Initialize(int canvasWidth, int canvasHeight)
        {
            CanvasWidth = canvasWidth;
            CanvasHeight = canvasHeight;
            
            CellSize = Math.Max(20, Math.Min(canvasWidth / 30, canvasHeight / 20));
            GridWidth = canvasWidth / CellSize;
            GridHeight = canvasHeight / CellSize;
            
            CanvasWidth = GridWidth * CellSize;
            CanvasHeight = GridHeight * CellSize;
            
            _snake = new Snake(GridWidth / 2, GridHeight / 2);
            
            SpawnFood();
            StartGameLoop();
        }

        private void StartGameLoop()
        {
            _gameLoop?.Dispose();
            _gameLoop = Observable.Interval(TimeSpan.FromMilliseconds(_settings.GameSpeed))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Update());
        }

        public void Cleanup()
        {
            _gameLoop?.Dispose();
            _radioService.Stop();
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
                if (newHead.X < 0) newHead.X = GridWidth - 1;
                if (newHead.X >= GridWidth) newHead.X = 0;
                if (newHead.Y < 0) newHead.Y = GridHeight - 1;
                if (newHead.Y >= GridHeight) newHead.Y = 0;
            }
            else
            {
                if (newHead.X < 0 || newHead.X >= GridWidth || 
                    newHead.Y < 0 || newHead.Y >= GridHeight)
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
                _hue = (_hue + 0.02 * _settings.RgbSpeed) % 1.0;
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
                    X = _random.Next(0, GridWidth),
                    Y = _random.Next(0, GridHeight)
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
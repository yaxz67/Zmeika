using System.Reactive;
using ReactiveUI;
using Zmeika.Models;

namespace Zmeika.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private bool _enableTeleport = true;
        public bool EnableTeleport
        {
            get => _enableTeleport;
            set => this.RaiseAndSetIfChanged(ref _enableTeleport, value);
        }

        private bool _rgbSnake;
        public bool RgbSnake
        {
            get => _rgbSnake;
            set => this.RaiseAndSetIfChanged(ref _rgbSnake, value);
        }

        public ReactiveCommand<Unit, GameSettings> StartGameCommand { get; }

        public MainMenuViewModel()
        {
            StartGameCommand = ReactiveCommand.Create(() => new GameSettings
            {
                EnableTeleport = EnableTeleport,
                RgbSnake = RgbSnake,
                GameSpeed = 150
            });
        }
    }
}
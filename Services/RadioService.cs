using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LibVLCSharp.Shared;

namespace Zmeika.Services
{
    public class RadioStation
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Emoji { get; set; }
    }
    
    public class RadioService : IDisposable
    {
        private readonly List<RadioStation> _stations = new List<RadioStation>
        {
            new RadioStation { Name = "Радио Рекорд", Url = "https://radiorecord.hostingradio.ru/rr_main96.aacp", Emoji = "🎵" },
            new RadioStation { Name = "Европа Плюс", Url = "https://ep256.hostingradio.ru:8052/europaplus256.mp3", Emoji = "🎧" },
            new RadioStation { Name = "Русское Радио", Url = "https://rusradio.hostingradio.ru/rusradio96.aacp", Emoji = "🎸" },
            new RadioStation { Name = "DFM", Url = "https://dfm.hostingradio.ru/dfm96.aacp", Emoji = "🎹" },
            new RadioStation { Name = "Шансон", Url = "https://chanson.hostingradio.ru:8041/chanson256.mp3", Emoji = "🎤" },
        };
        
        private LibVLC _libVlc;
        private MediaPlayer _mediaPlayer;
        private int _currentStationIndex = -1;
        
        public List<RadioStation> Stations => _stations;
        public RadioStation CurrentStation => _currentStationIndex >= 0 ? _stations[_currentStationIndex] : null;
        public bool IsPlaying => _mediaPlayer?.IsPlaying ?? false;
        
        // Событие для обновления UI
        public event Action<string> StatusChanged;
        
        public RadioService()
        {
            Core.Initialize();
            _libVlc = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVlc);
            
            _mediaPlayer.EndReached += (s, e) =>
            {
                // Автоповтор при обрыве
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Thread.Sleep(2000);
                    if (_currentStationIndex >= 0)
                        PlayStation(_currentStationIndex);
                });
            };
        }
        
        public void PlayStation(int index)
        {
            if (index < 0 || index >= _stations.Count) return;
            
            Stop();
            _currentStationIndex = index;
            
            try
            {
                var media = new Media(_libVlc, new Uri(_stations[index].Url));
                _mediaPlayer.Play(media);
                StatusChanged?.Invoke($"{_stations[index].Emoji} {_stations[index].Name}");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"❌ Ошибка: {ex.Message}");
                _currentStationIndex = -1;
            }
        }
        
        public void Stop()
        {
            try
            {
                _mediaPlayer?.Stop();
            }
            catch { }
            _currentStationIndex = -1;
        }
        
        public void TogglePlay()
        {
            if (IsPlaying)
            {
                Stop();
                StatusChanged?.Invoke("Радио выключено");
            }
            else
            {
                if (_currentStationIndex >= 0)
                    PlayStation(_currentStationIndex);
                else
                    PlayStation(0);
            }
        }
        
        public void NextStation()
        {
            var nextIndex = (_currentStationIndex + 1) % _stations.Count;
            PlayStation(nextIndex);
        }
        
        public void PreviousStation()
        {
            var prevIndex = (_currentStationIndex - 1 + _stations.Count) % _stations.Count;
            PlayStation(prevIndex);
        }
        
        public void SetVolume(int volume)
        {
            if (_mediaPlayer != null)
                _mediaPlayer.Volume = Math.Clamp(volume, 0, 100);
        }
        
        public void Dispose()
        {
            Stop();
            _mediaPlayer?.Dispose();
            _libVlc?.Dispose();
        }
    }
}
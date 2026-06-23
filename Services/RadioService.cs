using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Zmeika.Services
{
    public class RadioStation
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Emoji { get; set; }
    }
    
    public class RadioService
    {
        private readonly List<RadioStation> _stations = new List<RadioStation>
        {
            new RadioStation { Name = "Радио Рекорд", Url = "https://radiorecord.hostingradio.ru/rr_main96.aacp", Emoji = "🎵" },
            new RadioStation { Name = "Европа Плюс", Url = "https://ep256.hostingradio.ru:8052/europaplus256.mp3", Emoji = "🎧" },
            new RadioStation { Name = "Русское Радио", Url = "https://rusradio.hostingradio.ru/rusradio96.aacp", Emoji = "🎸" },
            new RadioStation { Name = "DFM", Url = "https://dfm.hostingradio.ru/dfm96.aacp", Emoji = "🎹" },
            new RadioStation { Name = "Шансон", Url = "https://chanson.hostingradio.ru:8041/chanson256.mp3", Emoji = "🎤" }
        };
        
        private int _currentStationIndex = -1;
        private Process _vlcProcess;
        
        public List<RadioStation> Stations => _stations;
        public RadioStation CurrentStation => _currentStationIndex >= 0 ? _stations[_currentStationIndex] : null;
        
        public void PlayStation(int index)
        {
            Stop();
            
            if (index >= 0 && index < _stations.Count)
            {
                _currentStationIndex = index;
                
                try
                {
                    // Пробуем через VLC если установлен
                    _vlcProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "vlc",
                        Arguments = $"--intf dummy --play-and-exit {_stations[index].Url}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                catch
                {
                    // Если VLC нет - открываем в браузере
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = _stations[index].Url,
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        // Ничего не поделать
                    }
                }
            }
        }
        
        public void Stop()
        {
            try
            {
                _vlcProcess?.Kill();
                _vlcProcess?.Dispose();
                _vlcProcess = null;
            }
            catch { }
            _currentStationIndex = -1;
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
    }
}
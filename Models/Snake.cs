using System.Collections.ObjectModel;
using ReactiveUI;

namespace Zmeika.Models
{
    public class Snake : ReactiveObject
    {
        private ObservableCollection<Position> _body;
        public ObservableCollection<Position> Body
        {
            get => _body;
            set => this.RaiseAndSetIfChanged(ref _body, value);
        }

        private Direction _currentDirection;
        public Direction CurrentDirection
        {
            get => _currentDirection;
            set => this.RaiseAndSetIfChanged(ref _currentDirection, value);
        }

        public Snake(int startX, int startY)
        {
            _body = new ObservableCollection<Position>
            {
                new Position { X = startX, Y = startY },
                new Position { X = startX - 1, Y = startY },
                new Position { X = startX - 2, Y = startY }
            };
            _currentDirection = Direction.Right;
        }
    }
}
namespace Zmeika.Models
{
    public enum Direction
    {
        Up, Down, Left, Right
    }

    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class GameSettings
    {
        public bool EnableTeleport { get; set; } = true;
        public bool RgbSnake { get; set; } = false;
        public int GameSpeed { get; set; } = 150;
        public double RgbSpeed { get; set; } = 1.0; // Новая настройка
    }
}
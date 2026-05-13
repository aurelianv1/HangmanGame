using System.Collections.Generic;

namespace HangmanGame.Models
{
    public class GameState
    {
        public string Username { get; set; }
        public string Category { get; set; }
        public string Word { get; set; }
        public List<char> GuessedLetters { get; set; } = new List<char>();
        public int WrongGuesses { get; set; }
        public double TimeRemaining { get; set; }
        public int CurrentLevel { get; set; }
        public string SaveName { get; set; }
    }
}
using System.Collections.Generic;

namespace HangmanGame.Models
{
    public class CategoryStats
    {
        public string Category { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
    }

    public class Statistics
    {
        public string Username { get; set; }
        public List<CategoryStats> CategoryStats { get; set; } = new List<CategoryStats>();
    }
}
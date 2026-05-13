using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HangmanGame.Models;
using HangmanGame.Services;

namespace HangmanGame.Views
{
    public class StatRow
    {
        public string Username { get; set; }
        public string Category { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
    }

    public partial class StatisticsWindow : Window
    {
        public List<StatRow> AllStats { get; set; }

        public StatisticsWindow()
        {
            InitializeComponent();
            var service = new StatisticsService();
            var stats = service.LoadStatistics();

            AllStats = stats.SelectMany(s =>
                s.CategoryStats.Select(c => new StatRow
                {
                    Username = s.Username,
                    Category = c.Category,
                    GamesPlayed = c.GamesPlayed,
                    GamesWon = c.GamesWon
                })).ToList();

            DataContext = this;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
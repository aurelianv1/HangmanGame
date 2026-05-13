using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using HangmanGame.Models;

namespace HangmanGame.Services
{
    public class StatisticsService
    {
        private readonly string _filePath;

        public StatisticsService()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(baseDir, "Data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "statistics.json");
        }

        public List<Statistics> LoadStatistics()
        {
            if (!File.Exists(_filePath))
                return new List<Statistics>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Statistics>>(json) ?? new List<Statistics>();
        }

        public void SaveStatistics(List<Statistics> stats)
        {
            var json = JsonSerializer.Serialize(stats, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_filePath, json);
        }

        public void RecordGame(string username, string category, bool won)
        {
            var allStats = LoadStatistics();
            var userStats = allStats.FirstOrDefault(s => s.Username == username);

            if (userStats == null)
            {
                userStats = new Statistics { Username = username };
                allStats.Add(userStats);
            }

            var catStats = userStats.CategoryStats
                .FirstOrDefault(c => c.Category == category);

            if (catStats == null)
            {
                catStats = new CategoryStats { Category = category };
                userStats.CategoryStats.Add(catStats);
            }

            catStats.GamesPlayed++;
            if (won) catStats.GamesWon++;

            SaveStatistics(allStats);
        }

        public void DeleteUserStatistics(string username)
        {
            var allStats = LoadStatistics();
            allStats.RemoveAll(s => s.Username == username);
            SaveStatistics(allStats);
        }
    }
}
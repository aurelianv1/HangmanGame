using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using HangmanGame.Models;

namespace HangmanGame.Services
{
    public class GameService
    {
        private readonly string _wordsFile;
        private readonly string _savesFolder;
        private Dictionary<string, List<string>> _words;

        public GameService()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(baseDir, "Data");
            Directory.CreateDirectory(dataDir);
            _savesFolder = Path.Combine(dataDir, "Saves");
            Directory.CreateDirectory(_savesFolder);
            _wordsFile = Path.Combine(dataDir, "words.json");
            LoadWords();
        }

        private void LoadWords()
        {
            if (!File.Exists(_wordsFile))
            {
                _words = new Dictionary<string, List<string>>();
                return;
            }
            var json = File.ReadAllText(_wordsFile);
            _words = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json)
                     ?? new Dictionary<string, List<string>>();
        }

        public List<string> GetCategories()
        {
            return _words.Keys.ToList();
        }

        public string GetRandomWord(string category)
        {
            if (category == "All Categories")
            {
                var all = _words.Values.SelectMany(w => w).ToList();
                return all[new Random().Next(all.Count)];
            }

            if (_words.ContainsKey(category))
            {
                var list = _words[category];
                return list[new Random().Next(list.Count)];
            }

            return "HANGMAN";
        }

        public void SaveGame(GameState state)
        {
            var path = Path.Combine(_savesFolder, $"{state.Username}_{state.SaveName}.json");
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(path, json);
        }

        public List<GameState> LoadUserGames(string username)
        {
            var games = new List<GameState>();
            var files = Directory.GetFiles(_savesFolder, $"{username}_*.json");

            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var state = JsonSerializer.Deserialize<GameState>(json);
                if (state != null) games.Add(state);
            }

            return games;
        }

        public void DeleteUserGames(string username)
        {
            var files = Directory.GetFiles(_savesFolder, $"{username}_*.json");
            foreach (var file in files)
                File.Delete(file);
        }

        public void DeleteSave(string username, string saveName)
        {
            var path = Path.Combine(_savesFolder, $"{username}_{saveName}.json");
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
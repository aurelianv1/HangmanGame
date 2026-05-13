using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using HangmanGame.Models;

namespace HangmanGame.Services
{
    public class UserService
    {
        private readonly string _filePath;

        public UserService()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(baseDir, "Data");
            Directory.CreateDirectory(dataDir);
            Directory.CreateDirectory(Path.Combine(dataDir, "Images"));
            _filePath = Path.Combine(dataDir, "users.json");
        }

        public List<User> LoadUsers()
        {
            if (!File.Exists(_filePath))
                return new List<User>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public void SaveUsers(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_filePath, json);
        }

        public void AddUser(User user)
        {
            var users = LoadUsers();
            users.Add(user);
            SaveUsers(users);
        }

        public void DeleteUser(string username)
        {
            var users = LoadUsers();
            users.RemoveAll(u => u.Username == username);
            SaveUsers(users);
        }

        public bool UserExists(string username)
        {
            var users = LoadUsers();
            return users.Exists(u => u.Username == username);
        }
    }
}
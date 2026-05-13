using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using HangmanGame.Commands;
using HangmanGame.Models;
using HangmanGame.Services;

namespace HangmanGame.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private readonly GameService _gameService;
        private readonly StatisticsService _statisticsService;
        private readonly User _currentUser;
        private DispatcherTimer _gameTimer;
        private string _currentWord;
        private List<char> _guessedLetters;
        private int _levelCount;
        private const int MaxWrongGuesses = 6;
        private const double GameDuration = 30;

        private string _displayWord;
        public string DisplayWord
        {
            get => _displayWord;
            set => SetProperty(ref _displayWord, value);
        }

        private string _category;
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        private int _currentLevel;
        public int CurrentLevel
        {
            get => _currentLevel;
            set => SetProperty(ref _currentLevel, value);
        }

        private int _wrongGuesses;
        public int WrongGuesses
        {
            get => _wrongGuesses;
            set => SetProperty(ref _wrongGuesses, value);
        }

        private double _timeRemaining;
        public double TimeRemaining
        {
            get => _timeRemaining;
            set => SetProperty(ref _timeRemaining, value);
        }

        private string _userImage;
        public string UserImage
        {
            get => _userImage;
            set => SetProperty(ref _userImage, value);
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _hangmanImage;
        public string HangmanImage
        {
            get => _hangmanImage;
            set => SetProperty(ref _hangmanImage, value);
        }

        private bool _gameInProgress;
        public bool GameInProgress
        {
            get => _gameInProgress;
            set => SetProperty(ref _gameInProgress, value);
        }

        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<char> LetterButtons { get; set; }

        public ICommand NewGameCommand { get; }
        public ICommand GuessLetterCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand OpenGameCommand { get; }
        public ICommand BackToLoginCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        public ICommand ShowStatisticsCommand { get; }
        public ICommand ShowAboutCommand { get; }

        public GameViewModel(User user)
        {
            _currentUser = user;
            _gameService = new GameService();
            _statisticsService = new StatisticsService();

            Username = user.Username;
            UserImage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, user.ImagePath);

            Categories = new ObservableCollection<string>(_gameService.GetCategories());
            Categories.Insert(0, "All Categories");
            Category = "All Categories";

            LetterButtons = new ObservableCollection<char>();
            InitializeLetters();

            NewGameCommand = new RelayCommand(_ => StartNewGame());
            GuessLetterCommand = new RelayCommand(letter => GuessLetter((char)letter));
            SaveGameCommand = new RelayCommand(_ => SaveGame(), _ => GameInProgress);
            OpenGameCommand = new RelayCommand(_ => OpenGame());
            BackToLoginCommand = new RelayCommand(_ => BackToLogin());
            SelectCategoryCommand = new RelayCommand(cat => SelectCategory((string)cat));
            ShowStatisticsCommand = new RelayCommand(_ => new Views.StatisticsWindow().ShowDialog());
            ShowAboutCommand = new RelayCommand(_ => new Views.AboutWindow().ShowDialog());
        }

        private void InitializeLetters()
        {
            for (char c = 'A'; c <= 'Z'; c++)
                LetterButtons.Add(c);
        }

        private void SelectCategory(string category)
        {
            Category = category;
            _levelCount = 0;
            CurrentLevel = 1;
        }

        private void StartNewGame()
        {
            _guessedLetters = new List<char>();
            WrongGuesses = 0;
            TimeRemaining = GameDuration;
            _currentWord = _gameService.GetRandomWord(Category).ToUpper();
            UpdateDisplayWord();
            UpdateHangmanImage();
            LetterButtons.Clear();
            InitializeLetters();
            GameInProgress = true;
            StartTimer();
        }

        private void UpdateDisplayWord()
        {
            var display = new List<char>();
            foreach (var c in _currentWord)
                display.Add(_guessedLetters.Contains(c) ? c : '_');
            DisplayWord = string.Join(" ", display);
        }

        private void GuessLetter(char letter)
        {
            if (!GameInProgress || _guessedLetters.Contains(letter))
                return;

            _guessedLetters.Add(letter);
            LetterButtons.Remove(letter);

            if (!_currentWord.Contains(letter))
            {
                WrongGuesses++;
                UpdateHangmanImage();
            }

            UpdateDisplayWord();

            if (WrongGuesses >= MaxWrongGuesses)
                GameLost();
            else if (_currentWord.All(c => _guessedLetters.Contains(c)))
                GameWon();
        }

        private void UpdateHangmanImage()
        {
            HangmanImage = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", "Images", $"hangman{WrongGuesses}.png");
        }

        private void GameWon()
        {
            _gameTimer?.Stop();
            GameInProgress = false;
            _levelCount++;
            CurrentLevel = _levelCount;

            if (_levelCount >= 3)
            {
                MessageBox.Show($"Felicitări {Username}! Ai câștigat jocul!",
                    "Victorie", MessageBoxButton.OK, MessageBoxImage.Information);
                _statisticsService.RecordGame(Username, Category, true);
                _levelCount = 0;
                CurrentLevel = 1;
            }
            else
            {
                MessageBox.Show($"Nivel câștigat! {_levelCount}/3 nivele completate.",
                    "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GameLost()
        {
            _gameTimer?.Stop();
            GameInProgress = false;
            _statisticsService.RecordGame(Username, Category, false);
            _levelCount = 0;
            CurrentLevel = 1;

            MessageBox.Show($"Jocul e pierdut! Cuvântul era: {_currentWord}",
                "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StartTimer()
        {
            _gameTimer?.Stop();
            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _gameTimer.Tick += (s, e) =>
            {
                TimeRemaining = Math.Round(TimeRemaining - 0.1, 1);
                if (TimeRemaining <= 0)
                {
                    TimeRemaining = 0;
                    _gameTimer.Stop();
                    GameLost();
                }
            };
            _gameTimer.Start();
        }

        private void SaveGame()
        {
            var gameState = new GameState
            {
                Username = Username,
                Category = Category,
                Word = _currentWord,
                GuessedLetters = _guessedLetters,
                WrongGuesses = WrongGuesses,
                TimeRemaining = TimeRemaining,
                CurrentLevel = _levelCount,
                SaveName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
            };

            _gameTimer?.Stop();
            _gameService.SaveGame(gameState);
            MessageBox.Show("Jocul a fost salvat!", "Succes",
                MessageBoxButton.OK, MessageBoxImage.Information);
            _gameTimer?.Start();
        }

        private void OpenGame()
        {
            var savedGames = _gameService.LoadUserGames(Username);
            if (savedGames.Count == 0)
            {
                MessageBox.Show("Nu există jocuri salvate!", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Views.OpenGameDialog(savedGames);
            if (dialog.ShowDialog() == true && dialog.SelectedGame != null)
                LoadGameState(dialog.SelectedGame);
        }

        private void LoadGameState(GameState state)
        {
            _gameTimer?.Stop();
            _currentWord = state.Word;
            Category = state.Category;
            _guessedLetters = state.GuessedLetters;
            WrongGuesses = state.WrongGuesses;
            TimeRemaining = state.TimeRemaining;
            _levelCount = state.CurrentLevel;
            CurrentLevel = state.CurrentLevel;

            _gameService.DeleteSave(Username, state.SaveName);

            UpdateDisplayWord();
            UpdateHangmanImage();

            LetterButtons.Clear();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (!_guessedLetters.Contains(c))
                    LetterButtons.Add(c);
            }

            GameInProgress = true;
            StartTimer();
        }

        private void BackToLogin()
        {
            _gameTimer?.Stop();
            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Views.GameWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

    }
}
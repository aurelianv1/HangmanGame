using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using HangmanGame.Commands;
using HangmanGame.Models;
using HangmanGame.Services;

namespace HangmanGame.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly UserService _userService;

        public ObservableCollection<User> Users { get; set; }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                OnPropertyChanged(nameof(IsUserSelected));
                OnPropertyChanged(nameof(PreviewImagePath));
            }
        }

        public bool IsUserSelected => SelectedUser != null;

        private string _newUsername;
        public string NewUsername
        {
            get => _newUsername;
            set => SetProperty(ref _newUsername, value);
        }

        private string _selectedImagePath;
        public string SelectedImagePath
        {
            get => _selectedImagePath;
            set
            {
                SetProperty(ref _selectedImagePath, value);
                OnPropertyChanged(nameof(PreviewImagePath));
            }
        }

        public string PreviewImagePath
        {
            get
            {
                var path = SelectedImagePath ?? SelectedUser?.ImagePath;
                if (path == null) return null;

                if (System.IO.Path.IsPathRooted(path))
                    return path;

                return System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    path);
            }
        }

        private readonly List<string> _predefinedImages = new List<string>
        {
            "Data/Images/img1.png",
            "Data/Images/img2.png",
            "Data/Images/img3.png"
        };

        private int _imageIndex = 0;

        public ICommand PlayCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand NewUserCommand { get; }
        public ICommand BrowseImageCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand PrevImageCommand { get; }
        public ICommand NextImageCommand { get; }

        public LoginViewModel()
        {
            _userService = new UserService();
            Users = new ObservableCollection<User>(_userService.LoadUsers());

            PlayCommand = new RelayCommand(_ => OpenGame(), _ => IsUserSelected);
            DeleteUserCommand = new RelayCommand(_ => DeleteUser(), _ => IsUserSelected);
            NewUserCommand = new RelayCommand(_ => AddNewUser(), _ => !string.IsNullOrWhiteSpace(NewUsername));
            BrowseImageCommand = new RelayCommand(_ => BrowseImage());
            CancelCommand = new RelayCommand(_ => Application.Current.Shutdown());
            PrevImageCommand = new RelayCommand(_ => PrevImage());
            NextImageCommand = new RelayCommand(_ => NextImage());
        }

        private void OpenGame()
        {
            var gameWindow = new Views.GameWindow(SelectedUser);
            gameWindow.Show();
            Application.Current.MainWindow.Close();
        }

        private void DeleteUser()
        {
            var result = MessageBox.Show(
                $"Ești sigur că vrei să ștergi utilizatorul {SelectedUser.Username}?",
                "Confirmare", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _userService.DeleteUser(SelectedUser.Username);
                new StatisticsService().DeleteUserStatistics(SelectedUser.Username);
                new GameService().DeleteUserGames(SelectedUser.Username);
                Users.Remove(SelectedUser);
                SelectedUser = null;
            }
        }

        private void AddNewUser()
        {
            if (_userService.UserExists(NewUsername))
            {
                MessageBox.Show("Utilizatorul există deja!", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var user = new User
            {
                Username = NewUsername,
                ImagePath = SelectedImagePath ?? "Data/Images/default.png"
            };

            _userService.AddUser(user);
            Users.Add(user);
            NewUsername = string.Empty;
            SelectedImagePath = null;
        }

        private void BrowseImage()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.gif;*.png)|*.jpg;*.gif;*.png",
                Title = "Alege o imagine"
            };

            if (dialog.ShowDialog() == true)
            {
                var relativePath = System.IO.Path.GetRelativePath(
                    System.AppDomain.CurrentDomain.BaseDirectory,
                    dialog.FileName);
                SelectedImagePath = relativePath;
            }
        }

        private void PrevImage()
        {
            _imageIndex = (_imageIndex - 1 + _predefinedImages.Count) % _predefinedImages.Count;
            SelectedImagePath = _predefinedImages[_imageIndex];
        }

        private void NextImage()
        {
            _imageIndex = (_imageIndex + 1) % _predefinedImages.Count;
            SelectedImagePath = _predefinedImages[_imageIndex];
        }
    }
}
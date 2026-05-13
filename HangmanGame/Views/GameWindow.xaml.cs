using System.Windows;
using System.Windows.Input;
using HangmanGame.Models;
using HangmanGame.ViewModels;

namespace HangmanGame.Views
{
    public partial class GameWindow : Window
    {
        private GameViewModel _viewModel;

        public GameWindow(User user)
        {
            InitializeComponent();
            _viewModel = new GameViewModel(user);
            DataContext = _viewModel;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                char letter = (char)('A' + (e.Key - Key.A));
                _viewModel.GuessLetterCommand.Execute(letter);
            }
        }
    }
}
using System.Collections.Generic;
using System.Windows;
using HangmanGame.Models;

namespace HangmanGame.Views
{
    public partial class OpenGameDialog : Window
    {
        public GameState SelectedGame { get; private set; }
        public List<GameState> SavedGames { get; set; }

        public OpenGameDialog(List<GameState> savedGames)
        {
            InitializeComponent();
            SavedGames = savedGames;
            DataContext = this;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedGame = SavedGamesList.SelectedItem as GameState;
            if (SelectedGame == null)
            {
                MessageBox.Show("Selectează un joc!", "Atenție",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
using System.Windows;
using Hangman.WPF.ViewModels;

namespace Hangman.WPF.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // NY METOD: Denna körs när fönstret har laddats (från XAML)
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Hämta vår ViewModel (som skapades i XAML)
            var viewModel = DataContext as GameViewModel;
            if (viewModel != null)
            {
                // Anropa den nya asynkrona laddningsmetoden
                await viewModel.LoadNewGameAsync();
            }
        }

        // Denna metod hade du sedan tidigare
        private void GuessButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Hämta vår ViewModel
            var viewModel = DataContext as GameViewModel;
            if (viewModel == null) return;

            // 2. Hämta texten från TextBox
            string guess = GuessTextBox.Text;

            // 3. Validera och skicka till ViewModel
            if (!string.IsNullOrEmpty(guess))
            {
                viewModel.MakeGuess(guess.ToUpper()[0]);
            }

            // 4. Rensa och fokusera rutan
            GuessTextBox.Clear();
            GuessTextBox.Focus();
        }
    }
}
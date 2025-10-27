using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hangman.WPF.ViewModels;

namespace Hangman.WPF.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // (DataContext sätts nu i XAML-filen,
            //  så vi behöver inte göra det här)
        }

        // Denna metod körs när knappen i XAML klickas
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
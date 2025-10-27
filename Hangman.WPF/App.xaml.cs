using System.Windows;
using Hangman.Core.Providers.Db;
using Hangman.Core.Providers.Interface;
using Hangman.WPF.ViewModels;
using Hangman.WPF.Views;

namespace Hangman.WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Skapa våra tjänster (samma som i Console/Program.cs)
            IStatisticsService statisticsService = new SqliteHangmanService();

            // 2. Skapa vår "Hjärna" (MainViewModel) och skicka in tjänsterna
            var mainViewModel = new MainViewModel(statisticsService);

            // 3. Skapa huvudfönstret och koppla det till vår MainViewModel
            var window = new MainWindow
            {
                DataContext = mainViewModel
            };

            window.Show();
        }
    }
}
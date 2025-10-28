using System.Windows;
using Hangman.Core.Providers.Db;
using Hangman.Core.Providers.Interface;
using Hangman.WPF.ViewModels;
using Hangman.WPF.Views;
using Hangman.Core.Localizations;

namespace Hangman.WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IStatisticsService statisticsService = new SqliteHangmanService();

            IUiStrings swedishStrings = new SwedishUiStrings();
            IUiStrings englishStrings = new EnglishUiStrings();

            var localizationProvider = new LocalizationProvider(englishStrings);

            var mainViewModel = new MainViewModel(statisticsService, localizationProvider);

            var window = new MainWindow
            {
                DataContext = mainViewModel
            };

            window.Show();
        }
    }
}
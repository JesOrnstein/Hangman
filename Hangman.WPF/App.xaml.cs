using System.Windows;
using Hangman.Core.Providers.Db;
using Hangman.Core.Providers.Interface;
using Hangman.WPF.ViewModels;
using Hangman.WPF.Views;
using Hangman.Core.Localizations; // <-- NY USING

namespace Hangman.WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Skapa våra tjänster (samma som förut)
            IStatisticsService statisticsService = new SqliteHangmanService();

            // 2. SKAPA VÅRA SPRÅK-STRATEGIER (NYTT!)
            //    (Förutsätter att de nu ligger i Hangman.Core.Localizations)
            IUiStrings swedishStrings = new SwedishUiStrings();
            IUiStrings englishStrings = new EnglishUiStrings();

            // 3. SKAPA VÅR PROVIDER (NYTT!)
            //    Vi startar appen med svenska som standard
            var localizationProvider = new LocalizationProvider(englishStrings);

            // 4. Skapa vår "Hjärna" (MainViewModel) och INJICERA beroendena
            //    (Konstruktorn är nu ändrad)
            var mainViewModel = new MainViewModel(statisticsService, localizationProvider);

            // 5. Skapa huvudfönstret (samma som förut)
            var window = new MainWindow
            {
                DataContext = mainViewModel
            };

            window.Show();
        }
    }
}
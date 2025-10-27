using Hangman.Console;
using System;
using System.Threading.Tasks;
using Hangman.Core.Providers.Db;
using Hangman.Core.Providers.Interface;
using Hangman.Console.Localizations;

class Program
{
    static async Task Main()
    {
        try
        {
            // 1. VÄLJ SPRÅKSTRATEGI
            IUiStrings uiStrings = SelectLanguage();

            // 2. SKAPA ANDRA BEROENDEN
            IStatisticsService statsService = new SqliteHangmanService();

            // 3. Skapa UI och INJICERA BÅDA beroendena
            ConsoleUi ui = new ConsoleUi(statsService, uiStrings);

            // 4. Kör UI:t
            await ui.RunAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ett allvarligt, oväntat fel inträffade:");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }

        Console.WriteLine("\nProgrammet har avslutats. Tryck valfri tangent.");
        Console.ReadKey();
    }

    /// <summary>
    /// Enkel metod för att välja språk innan applikationen startar.
    /// </summary>
    private static IUiStrings SelectLanguage()
    {
        Console.Clear();
        Console.WriteLine("Välj språk / Select language:");
        Console.WriteLine("1. Svenska");
        Console.WriteLine("2. English");
        Console.Write("Ditt val / Your choice (1-2): ");

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.KeyChar == '1')
            {
                Console.WriteLine("1");
                return new SwedishUiStrings();
            }
            if (key.KeyChar == '2')
            {
                Console.WriteLine("2");
                return new EnglishUiStrings();
            }
        }
    }
}
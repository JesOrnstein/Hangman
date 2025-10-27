using Hangman.IO;
using System;
using System.Threading.Tasks;
using Hangman.Core.Providers.Db;
using Hangman.Core.Providers.Interface;

class Program
{
    static async Task Main()
    {
        try
        {
            // 1. SKAPA BEROENDENA (COMPOSITION ROOT)
            // Här väljer vi den konkreta implementeringen (SQLite)
            IStatisticsService statsService = new SqliteHangmanService();

            // 2. Skapa vårt UI och INJICERA beroendet
            // Detta anropar nu den nya konstruktorn i ConsoleUi(IStatisticsService)
            ConsoleUi ui = new ConsoleUi(statsService);

            // 3. Kör UI:t (som nu innehåller huvudmenyn)
            await ui.RunAsync();
        }
        catch (Exception ex)
        {
            // Felhanteringen stannar kvar här som ett yttersta skyddsnät
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ett allvarligt, oväntat fel inträffade:");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }

        Console.WriteLine("\nProgrammet har avslutats. Tryck valfri tangent.");
        Console.ReadKey();
    }
}
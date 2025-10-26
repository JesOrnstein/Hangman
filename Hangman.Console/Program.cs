using Hangman.IO;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        try
        {
            // 1. Skapa vårt UI. Den sköter nu allt,
            //    inklusive menyval och skapande av Game/Provider.
            ConsoleUi ui = new ConsoleUi();

            // 2. Kör UI:t (som nu innehåller huvudmenyn)
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
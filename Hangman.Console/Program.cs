using Hangman.Core;
using Hangman.Core.Providers;
using System;
using System.Threading.Tasks;

class Program
{
    // 2. Ändra Main till att vara "async Task"
    static async Task Main()
    {
        try
        {
            // 3. Använd ditt nya asynkrona interface och provider
            IAsyncWordProvider provider = new ApiWordProvider();

            Console.WriteLine("Hämtar ett ord från API:et, vänligen vänta...");

            //Skapa spelet (säg t.ex. 6 fel tillåtna)
            Game game = new Game(maxMistakes: 6);

            // 4. Hämta ordet asynkront med "await"
            string secret = await provider.GetWordAsync();

            //Starta spelet med det ordet
            game.StartNew(secret);

            Console.WriteLine($"Spelet är startat! Ordet är {secret.Length} bokstäver långt.");
            Console.WriteLine($"(Ordkälla: {provider.DifficultyName})");
            Console.WriteLine($"?\nLyckades! Hämtat ordet: {secret}");

            // TODO: Här kommer din framtida spell-loop (ConsoleUi)
            // Just nu avslutas programmet direkt efter start.
        }
        catch (Exception ex)
        {
            // 5. Fånga fel om API:et eller nätverket misslyckas
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ett allvarligt fel inträffade vid start:");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }

        Console.WriteLine("\nTryck valfri tangent för att avsluta...");
        Console.ReadKey();
    }
}
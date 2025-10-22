using Hangman.Core;
using System.Globalization;

namespace Hangman.IO;
/*
  Sköter all in- och utmatning i konsolen.
  - Visar ordet, felantal, och vilka bokstäver som gissats.
  - Tar emot användarens bokstavsgissningar.
  - Ritar upp gubben i ASCII steg för steg.
  - Färglägger meddelanden (rätt = grönt, fel = gult/rött).
  - Använder Game för att styra spelets gång.
*/

public sealed class ConsoleUi
{
    private readonly Game _game;
    private readonly IWordProvider _wordProvider;

    public ConsoleUi(Game game, IWordProvider wordProvider)
    {
        _game = game;
        _wordProvider = wordProvider;

        _game.LetterGuessed += (_, c) => WriteInfo($"✔ Rätt: {c}");
        _game.WrongLetterGuessed += (_, c) => WriteWarn($"✖ Fel: {c}");
        _game.GameEnded += (_, status) =>
        {
            if (status == GameStatus.Won) WriteSuccess("🎉 Du vann!");
            else WriteError("💀 Du förlorade.");
        };
    }

    public async Task RunAsync()
    {
        WriteTitle("HÄNGA GUBBE – C# Console (sv-SE)");
        WriteInfo($"Svårighet: {_wordProvider.DifficultyName}");
        var play = true;
        while (play)
        {
            _game.StartNew(_wordProvider.GetWord());
            await PlayRoundAsync();
            play = AskYesNo("Spela igen? (J/N): ");
        }
        WriteInfo("Hejdå!");
    }

    private async Task PlayRoundAsync()
    {
        while (_game.Status == GameStatus.InProgress)
        {
            DrawBoard();

            var input = ReadLetter("Gissa en bokstav: ");
            if (input is null) continue;

            _game.Guess(input.Value);
            await Task.Delay(250); // liten paus för känsla
        }

        DrawBoard(final: true);
        Console.WriteLine($"Ordet var: {_game.Secret}");
    }

    private void DrawBoard(bool final = false)
    {
        Console.WriteLine();
        Console.WriteLine($"Ord:  {_game.GetMaskedWord()}");
        Console.WriteLine($"Fel:  {_game.Mistakes} / {_game.AttemptsLeft + _game.Mistakes}  (kvar: {_game.AttemptsLeft})");

        // Visar både rätt och fel gissade bokstäver
        Console.WriteLine($"Gissat (rätt): [{string.Join(' ', _game.UsedLetters)}]");
        Console.WriteLine($"Gissat (fel):  [{string.Join(' ', _game.WrongLetters)}]");

        DrawHangman(_game.Mistakes);
        Console.WriteLine();
    }

    private static void DrawHangman(int mistakes)
    {
        // 7 steg (0..6) för att matcha maxMistakes = 6.
        // Galgen visas alltid; sedan läggs delar till: huvud, kropp, armar, ben.
        string[] stages = new[]
        {
            // 0: tom galge
            "  +---+\n  |   |\n      |\n      |\n      |\n=======",
            // 1: huvud
            "  +---+\n  |   |\n  O   |\n      |\n      |\n=======",
            // 2: kropp
            "  +---+\n  |   |\n  O   |\n  |   |\n      |\n=======",
            // 3: vänster arm
            "  +---+\n  |   |\n  O   |\n /|   |\n      |\n=======",
            // 4: höger arm
            "  +---+\n  |   |\n  O   |\n /|\\  |\n      |\n=======",
            // 5: vänster ben
            "  +---+\n  |   |\n  O   |\n /|\\  |\n /    |\n=======",
            // 6: höger ben (full gubbe)
            "  +---+\n  |   |\n  O   |\n /|\\  |\n / \\  |\n======="
        };
        var idx = Math.Clamp(mistakes, 0, stages.Length - 1);
        Console.WriteLine(stages[idx]);
    }

    private static bool AskYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var key = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();

            var c = char.ToUpper(key, new CultureInfo("sv-SE"));
            if (c is 'J' or 'Y') return true; // svenska/engelska
            if (c is 'N') return false;
            WriteWarn("Svara J (ja) eller N (nej).");
        }
    }

    private static char? ReadLetter(string prompt)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            WriteWarn("Ange en bokstav.");
            return null;
        }

        var ch = input.Trim()[0];
        var c = char.ToUpper(ch, new CultureInfo("sv-SE"));
        if (!char.IsLetter(c))
        {
            WriteWarn("Endast bokstäver A–Ö tillåtna.");
            return null;
        }
        return c;
    }

    private static void WriteTitle(string text)
        => WriteWithColor(ConsoleColor.Cyan, text);
    private static void WriteInfo(string text)
        => WriteWithColor(ConsoleColor.Gray, text);
    private static void WriteWarn(string text)
        => WriteWithColor(ConsoleColor.Yellow, text);
    private static void WriteSuccess(string text)
        => WriteWithColor(ConsoleColor.Green, text);
    private static void WriteError(string text)
        => WriteWithColor(ConsoleColor.Red, text);

    private static void WriteWithColor(ConsoleColor color, string text)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = prev;
    }
}

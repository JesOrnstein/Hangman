using System.Globalization;

namespace Hangman.Core;

/*
  Innehåller all spel-logik:
  - Startar nytt spel med StartNew()
  - Hanterar gissningar via Guess()
  - Håller reda på rätt/fel gissningar och status (vinst/förlust)
  - Returnerar det maskerade ordet (_ _ _ _)
  - Använder events (LetterGuessed, WrongLetterGuessed, GameEnded)
    för att meddela användargränssnittet när något händer.
*/

public sealed class Game
{
    private readonly int _maxMistakes;
    private readonly HashSet<char> _guessed = new();
    private readonly HashSet<char> _wrong = new();

    private string _secret = string.Empty;

    public Game(int maxMistakes = 6)
    {
        if (maxMistakes <= 0) throw new ArgumentOutOfRangeException(nameof(maxMistakes));
        _maxMistakes = maxMistakes;
    }

    public GameStatus Status { get; private set; } = GameStatus.InProgress;
    public int Mistakes => _wrong.Count;
    public int AttemptsLeft => _maxMistakes - Mistakes;
    public IReadOnlyCollection<char> UsedLetters => _guessed.OrderBy(c => c).ToArray();
    public IReadOnlyCollection<char> WrongLetters => _wrong.OrderBy(c => c).ToArray();

    public string Secret => _secret;

    public event EventHandler<char>? LetterGuessed;
    public event EventHandler<char>? WrongLetterGuessed;
    public event EventHandler<GameStatus>? GameEnded;

    public void StartNew(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            throw new ArgumentException("Secret word cannot be empty", nameof(word));

        _secret = NormalizeWord(word);
        _guessed.Clear();
        _wrong.Clear();
        Status = GameStatus.InProgress;
    }

    public bool Guess(char rawChar)
    {
        if (Status != GameStatus.InProgress) return false;

        var c = NormalizeChar(rawChar);
        if (!char.IsLetter(c)) return false;
        if (_guessed.Contains(c) || _wrong.Contains(c)) return false;

        if (_secret.Contains(c))
        {
            _guessed.Add(c);
            LetterGuessed?.Invoke(this, c);
            if (AllRevealed())
            {
                Status = GameStatus.Won;
                GameEnded?.Invoke(this, Status);
            }
            return true;
        }
        else
        {
            _wrong.Add(c);
            WrongLetterGuessed?.Invoke(this, c);
            if (Mistakes >= _maxMistakes)
            {
                Status = GameStatus.Lost;
                GameEnded?.Invoke(this, Status);
            }
            return false;
        }
    }

    public string GetMaskedWord()
    {
        if (string.IsNullOrEmpty(_secret)) return string.Empty;
        return string.Concat(_secret.Select(ch => _guessed.Contains(ch) ? ch : '_'));
    }

    private bool AllRevealed()
    {
        var needed = _secret.Where(char.IsLetter).Select(NormalizeChar).ToHashSet();
        return needed.IsSubsetOf(_guessed);
    }

    private static char NormalizeChar(char c)
        => char.ToUpper(c, new CultureInfo("sv-SE"));

    private static string NormalizeWord(string s)
        => string.Concat(s.ToUpper(new CultureInfo("sv-SE")));
}
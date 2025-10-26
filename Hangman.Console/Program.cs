using Hangman.Core;

class Program
{
    static void Main()
    {
        //Skapa din provider
        IWordProvider provider = new WordProvider();

        //Skapa spelet (säg t.ex. 6 fel tillåtna)
        Game game = new Game(maxMistakes: 6);

        //Hämta ett slumpmässigt ord från din provider
        string secret = provider.GetWord();

        //Starta spelet med det ordet
        game.StartNew(secret);

    }
}
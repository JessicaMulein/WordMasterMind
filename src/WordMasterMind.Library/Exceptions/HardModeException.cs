namespace WordMasterMind.Library.Exceptions;

public class HardModeException : Exception
{
    public readonly char Letter;

    public readonly int LetterPosition;
    public readonly bool Solved;

    public HardModeException(int letterPosition, char letter, bool solved) : base(message: Message(letter: letter,
        solved: solved))
    {
        this.LetterPosition = letterPosition;
        this.Letter = letter;
        this.Solved = solved;
    }

    private static string Message(char letter, bool solved)
    {
        return solved
            ? $"{letter} must remain in the correct position."
            : $"The word must contain the letter {letter}.";
    }
}
using GameEngine.Library.Helpers;

namespace GameEngine.Library.Exceptions;

public class HardModeException : GameEngineException
{
    public readonly char Letter;

    public readonly int LetterPosition;
    public readonly bool Solved;

    public HardModeException(int letterPosition, char letter, bool solved) : base(message: FormatMessage(
        letterPosition: letterPosition,
        letter: letter,
        solved: solved))
    {
        this.LetterPosition = letterPosition;
        this.Letter = letter;
        this.Solved = solved;
    }

    public static string FormatMessage(int letterPosition, char letter, bool solved)
    {
        return solved
            ? $"{Utilities.NumberToOrdinal(number: Utilities.HumanizeIndex(index: letterPosition))} letter must be {letter}"
            : $"Guess must contain {letter}";
    }
}
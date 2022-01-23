namespace WordMasterMind.Models;

public record AttemptDetail
{
    public readonly char Letter;
    public readonly bool LetterCorrect;
    public readonly bool PositionCorrect;

    public AttemptDetail(char letter, bool letterCorrect, bool positionCorrect)
    {
        Letter = letter;
        LetterCorrect = letterCorrect;
        PositionCorrect = positionCorrect;
    }
}
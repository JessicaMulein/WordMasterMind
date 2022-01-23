namespace WordMasterMind.Models;

public record AttemptDetail
{
    public readonly char Letter;
    public readonly bool LetterCorrect;
    public readonly bool PositionCorrect;

    public AttemptDetail(char letter, bool letterCorrect, bool positionCorrect)
    {
        this.Letter = letter;
        this.LetterCorrect = letterCorrect;
        this.PositionCorrect = positionCorrect;
    }
}
namespace WordMasterMind.Models;

public record AttemptDetail
{
    public readonly char Letter;
    public readonly bool LetterCorrect;
    public readonly int LetterPosition;
    public readonly bool PositionCorrect;

    public AttemptDetail(int letterPosition, char letter, bool letterCorrect, bool positionCorrect)
    {
        this.LetterPosition = letterPosition;
        this.Letter = letter;
        this.LetterCorrect = letterCorrect;
        this.PositionCorrect = positionCorrect;
    }

    public override string ToString()
    {
        return WordMasterMind.GetEmojiFromAttemptDetail(attemptDetail: this);
    }
}
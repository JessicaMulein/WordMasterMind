namespace WordMasterMind.Library.Models;

public record AttemptDetail
{
    public readonly char Letter;
    public readonly bool LetterCorrect;
    public readonly int LetterPosition;
    public readonly bool PositionCorrect;

    public AttemptDetail(int letterPosition, char letter, bool letterCorrect, bool positionCorrect)
    {
        LetterPosition = letterPosition;
        Letter = letter;
        LetterCorrect = letterCorrect;
        PositionCorrect = positionCorrect;
    }

    public override string ToString()
    {
        return WordMasterMindGame.GetEmojiFromAttemptDetail(attemptDetail: this);
    }
}
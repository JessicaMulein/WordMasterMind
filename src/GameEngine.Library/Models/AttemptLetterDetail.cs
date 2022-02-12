using GameEngine.Library.Enumerations;

namespace GameEngine.Library.Models;

public record AttemptLetterDetail
{
    public readonly LetterEvaluation Evaluation;
    public readonly char Letter;
    public readonly int LetterPosition;

    public AttemptLetterDetail(int letterPosition, char letter, LetterEvaluation evaluation)
    {
        this.LetterPosition = letterPosition;
        this.Letter = letter;
        this.Evaluation = evaluation;
    }

    public override string ToString()
    {
        return GameEngineInstance.GetEmojiFromAttemptDetail(attemptLetterDetail: this);
    }
}
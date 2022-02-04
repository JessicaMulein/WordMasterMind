using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Library.Models;

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
        return WordMasterMindGame.GetEmojiFromAttemptDetail(attemptLetterDetail: this);
    }

    public static AttemptLetterDetail FromEvaluation(LetterEvaluation evaluation, char letter, int letterPosition)
    {
        return new AttemptLetterDetail(
            letterPosition: letterPosition,
            letter: letter,
            evaluation: evaluation);
    }
}
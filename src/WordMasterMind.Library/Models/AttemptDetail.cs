using System.Diagnostics;
using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Library.Models;

public record AttemptDetail
{
    public readonly char Letter;
    public readonly bool LetterPresent;
    public readonly int LetterPosition;
    public readonly bool LetterCorrect;

    public LetterEvaluation Evaluation => this.LetterCorrect switch
    {
        true => LetterEvaluation.Correct,
        _ => this.LetterPresent ? LetterEvaluation.Present : LetterEvaluation.Absent,
    };

    public AttemptDetail(int letterPosition, char letter, bool letterPresent, bool letterCorrect)
    {
        this.LetterPosition = letterPosition;
        this.Letter = letter;
        this.LetterPresent = letterPresent;
        this.LetterCorrect = letterCorrect;
    }

    public override string ToString()
    {
        return WordMasterMindGame.GetEmojiFromAttemptDetail(attemptDetail: this);
    }

    public static AttemptDetail FromEvaluation(LetterEvaluation evaluation, char letter, int letterPosition) 
        => new AttemptDetail(
            letterPosition: letterPosition,
            letter: letter,
            letterPresent: evaluation is LetterEvaluation.Present or LetterEvaluation.Correct,
            letterCorrect: evaluation is LetterEvaluation.Correct);
}
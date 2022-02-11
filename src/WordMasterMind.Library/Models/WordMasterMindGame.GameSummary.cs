using System.Net;
using System.Text;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Helpers;

namespace WordMasterMind.Library.Models;

public partial class WordMasterMindGame
{
    public string AttemptHistoryEmojiString
    {
        get
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < this.CurrentAttempt; i++)
                // join all of the emojis for each attempt and append
                stringBuilder.AppendLine(
                    value: string.Concat(
                        values: this._attempts[i].Details
                            .Select(selector: a => a.ToString())));

            return stringBuilder.ToString();
        }
    }

    public string PuzzleHeader
    {
        get
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(value: $"Word MasterMind W{this.WordLength}:");
            var puzzleNumber = DailyWordGenerator.PuzzleNumberForWordOfTheDay(
                word: this.SecretWord,
                dictionary: this.LiteralDictionary);
            if (puzzleNumber >= 0)
            {
                stringBuilder.Append(value: $"P{puzzleNumber}");
            }
            else
            {
                var wordIndex = this.LiteralDictionary.IndexForWord(word: this.SecretWord);
                stringBuilder.Append(value: $"I{wordIndex}");
            }

            stringBuilder.Append(value: $" {this.Attempts.Count()}/{this.MaxAttempts}");
            if (this._hardMode)
                stringBuilder.Append(value: '*');
            return stringBuilder.ToString();
        }
    }

    public string AttemptHistoryString => string.Concat(
        values: new[]
        {
            this.PuzzleHeader,
            Environment.NewLine,
            this.AttemptHistoryEmojiString,
            Environment.NewLine,
        });

    /// <summary>
    ///     Game score. Higher is better.
    ///     TODO: https://github.com/WordMasterMind/WordMasterMind/discussions/2#discussioncomment-2144488
    /// </summary>
    public int Score
    {
        get
        {
            // two points per attempt under maximum
            var score = 2 * (this.MaxAttempts - this.CurrentAttempt);
            // three points if the first time a letter is used, it is in the correct position
            // one point for each new letter out of place
            // one more point for a previously guessed letter when it is in the correct position
            var newLetters = new List<char>();
            foreach (var turn in this._attempts)
            foreach (var attemptDetail in turn.Details)
            {
                var newLetter = !newLetters.Contains(item: attemptDetail.Letter);
                switch (attemptDetail.Evaluation)
                {
                    case LetterEvaluation.Correct:
                        score += newLetter ? 3 : 1;
                        newLetters.Add(item: attemptDetail.Letter);
                        break;
                    case LetterEvaluation.Present:
                        score += newLetter ? 1 : 0;
                        newLetters.Add(item: attemptDetail.Letter);
                        break;
                    case LetterEvaluation.Absent:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return score;
        }
    }

    private static string GetEmojiFromConst(in string constValue)
    {
        return WebUtility.HtmlDecode(value: constValue);
    }

    public static string GetEmojiFromAttemptDetail(in AttemptLetterDetail attemptLetterDetail)
    {
        var emojiColor = attemptLetterDetail.Evaluation switch
        {
            LetterEvaluation.Correct => Constants.GreenSquareEmoji,
            LetterEvaluation.Present => Constants.YellowSquareEmoji,
            _ => Constants.BlackSquareEmoji,
        };

        return GetEmojiFromConst(constValue: emojiColor);
    }

    public static string AttemptToEmojiString(IEnumerable<AttemptLetterDetail> attemptDetails)
    {
        var stringBuilder = new StringBuilder();
        foreach (var attemptDetail in attemptDetails)
            stringBuilder.Append(value: GetEmojiFromAttemptDetail(attemptLetterDetail: attemptDetail));

        stringBuilder.AppendLine();

        return stringBuilder.ToString();
    }
}
using GameEngine.Library.Enumerations;
using GameEngine.Library.Exceptions;
using GameEngine.Library.Helpers;

namespace GameEngine.Library.Models;

public partial class GameEngineInstance
{
    /// <summary>
    ///     Collection of attempts
    /// </summary>
    private readonly AttemptDetails[] _attempts;

    /// <summary>
    ///     How many attempts are allowed before the game is over
    /// </summary>
    public readonly int MaxAttempts;

    /// <summary>
    ///     Length of the word to be guessed
    /// </summary>
    public readonly int WordLength;

    /// <summary>
    ///     Gets the current attempt number
    /// </summary>
    public int CurrentAttempt { get; private set; }

    /// <summary>
    ///     Gets the attempts so far
    /// </summary>
    public IEnumerable<AttemptDetails> Attempts => this._attempts[..this.CurrentAttempt];

    public static int GetMaxAttemptsForLength(int length)
    {
        return length + 1;
    }

    public AttemptDetails GetAttempt(int attemptIndex)
    {
        if (attemptIndex < 0 || attemptIndex >= this.CurrentAttempt)
            throw new ArgumentOutOfRangeException(paramName: nameof(attemptIndex));

        return this._attempts[attemptIndex];
    }

    /// <summary>
    ///     Maximum number of times we will reveal finding a particular letter for a given secret word
    ///     Equals the number of times it appears in the secret word
    /// </summary>
    /// <param name="secretWord"></param>
    /// <param name="attemptLetter"></param>
    /// <returns></returns>
    private static int OccurrencesForLetter(string secretWord, char attemptLetter)
    {
        return secretWord.Count(predicate: t => t.Equals(obj: attemptLetter));
    }

    /// <summary>
    ///     Produce a letter evaluation for a given guess, following the rules of Wordle
    ///     Wordle will return as many matches as there are times that letter showed up,
    ///     but no moe. For example:
    ///     suppose the word is “elegy”, and user guesses “eerie”.
    ///     Wordle/Wordmaster should correctly only flag two ‘e’s
    /// </summary>
    /// <param name="secretWord"></param>
    /// <param name="wordAttempt"></param>
    /// <param name="letterIndex"></param>
    /// <returns></returns>
    public static LetterEvaluation EvaluateLetter(string secretWord, string wordAttempt, int letterIndex)
    {
        var attemptLetter = wordAttempt[index: letterIndex];

        // absent positions are absent, period.
        if (!secretWord.Contains(value: attemptLetter))
            return LetterEvaluation.Absent;

        // solved positions are solved, period.
        if (secretWord[index: letterIndex] == attemptLetter)
            return LetterEvaluation.Correct;

        // total count of appearances of this letter
        var maxFlagsAvailable = OccurrencesForLetter(
            secretWord: secretWord,
            attemptLetter: attemptLetter);

        // get count of solved instances of this letter
        var attemptLetterSolvedPositions = secretWord
            .Where(predicate: (t, index) => t.Equals(obj: wordAttempt[index: index]) &&
                                            t.Equals(obj: attemptLetter))
            .Count();

        // get count of present letters in the wrong place before this one (left to right)
        var occurrencesBeforeCurrentIndex = secretWord
            .Where(predicate: (t, index) => index < letterIndex &&
                                            !t.Equals(obj: wordAttempt[index: index]) &&
                                            t.Equals(obj: attemptLetter))
            .Count();

        var remainingFlags = maxFlagsAvailable - attemptLetterSolvedPositions - occurrencesBeforeCurrentIndex;

        return remainingFlags > 0
            ? LetterEvaluation.Present
            : LetterEvaluation.Absent;
    }

    /// <summary>
    ///     Attempt a guess at the secret word.
    ///     The engine will return an AttemptDetails with a list of letter evaluations, one for each letter in the secret word.
    /// </summary>
    /// <param name="wordAttempt"></param>
    /// <returns></returns>
    /// <exception cref="GameOverException"></exception>
    /// <exception cref="InvalidAttemptLengthException"></exception>
    /// <exception cref="NotInDictionaryException"></exception>
    /// <exception cref="HardModeException"></exception>
    public AttemptDetails MakeAttempt(string wordAttempt)
    {
        if (this.GameOver) throw new GameOverException(solved: this.Solved);

        if (this.WordLength != wordAttempt.Length)
            throw new InvalidAttemptLengthException();

        wordAttempt = wordAttempt.ToUpperInvariant();

        if (!this.LiteralDictionary.IsWord(word: wordAttempt))
            throw new NotInDictionaryException();

        // countAttemptLetterIndex is incremented each time the selector is fired, eg each letter
        var currentAttemptLetterIndex = 0;
        // the attempt hasn't been registered in the count yet
        this._attempts[this.CurrentAttempt] = new AttemptDetails(
            attemptNumber: Utilities.HumanizeIndex(index: this.CurrentAttempt),
            word: wordAttempt,
            details: wordAttempt
                .Select(
                    selector: c => new AttemptLetterDetail(
                        letterPosition: currentAttemptLetterIndex,
                        letter: c,
                        evaluation: EvaluateLetter(
                            secretWord: this._secretWord, 
                            wordAttempt: wordAttempt,
                            letterIndex: currentAttemptLetterIndex++)))
                .ToArray());

        // update solved and found letters arrays
        for (var i = 0; i < this.WordLength; i++)
        {
            var detail = this._attempts[this.CurrentAttempt].Details.ElementAt(index: i);
            this._solvedLetters[i] |= detail.Evaluation is LetterEvaluation.Correct;

            if (detail.Evaluation is LetterEvaluation.Present or LetterEvaluation.Correct &&
                !this._foundLetters.Contains(item: detail.Letter))
            {
                this._foundLetters.Add(item: detail.Letter);
                this._foundLetters.Sort();
            }
        }

        // the attempt hasn't been registered in the count yet, checking for being at least second turn
        if (this._hardMode && this.CurrentAttempt >= 1)
            for (var letterPosition = 0; letterPosition < this.WordLength; letterPosition++)
            {
                var originalLetter = this._secretWord[index: letterPosition];

                // if a previous attempt had a letter in the correct position, future attempts must have the same letter in the correct position
                /* if the letter has been previously solved and the letter has
                 * been changed from the secret word, throw the HardModeException
                 */
                if (this._solvedLetters[letterPosition] &&
                    this._attempts[this.CurrentAttempt].Details.ElementAt(index: letterPosition).Letter !=
                    this._secretWord[index: letterPosition])
                    throw new HardModeException(
                        letterPosition: letterPosition,
                        letter: originalLetter,
                        solved: true);

                // if a previous attempt has guessed a letter correctly, the current attempt must contain it
                if (this._foundLetters.Contains(item: originalLetter) &&
                    !wordAttempt.Contains(value: originalLetter))
                    throw new HardModeException(
                        letterPosition: letterPosition,
                        letter: originalLetter,
                        solved: false);
            }

        // if we haven't thrown an exception due to hard mode, and the word is the secret word, we've solved it
        if (this._secretWord.Equals(value: wordAttempt)) this.Solved = true;

        // return the current attempt's record and advance the counter
        return this._attempts[this.CurrentAttempt++];
    }
}

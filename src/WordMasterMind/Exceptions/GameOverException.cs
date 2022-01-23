namespace WordMasterMind.Exceptions;

public class GameOverException : Exception
{
    public const string SolvedText = "You have already solved this word!";
    public const string GameOverText = "Game Over: You have reached the maximum number of attempts.";
    public readonly bool Solved;
    public GameOverException(bool solved = false) : base(message: solved ? SolvedText : GameOverText)
    {
        this.Solved = solved;
    }
}
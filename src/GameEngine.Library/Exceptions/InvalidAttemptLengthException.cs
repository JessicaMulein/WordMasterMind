namespace GameEngine.Library.Exceptions;

public class InvalidAttemptLengthException : GameEngineException
{
    public const string MessageText = "Word length does not match secret word length.";

    public InvalidAttemptLengthException() : base(message: MessageText)
    {
    }
}
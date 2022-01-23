namespace WordMasterMind.Exceptions;

public class InvalidAttemptLengthException : Exception
{
    public const string MessageText = "Word length does not match secret word length.";

    public InvalidAttemptLengthException() : base(message: MessageText)
    {
    }
}
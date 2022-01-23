namespace WordMasterMind.Exceptions;

public class InvalidWordLengthException : Exception
{
    public const string MessageText = "Word length does not match secret word length";

    public InvalidWordLengthException() : base(message: MessageText)
    {
    }
}
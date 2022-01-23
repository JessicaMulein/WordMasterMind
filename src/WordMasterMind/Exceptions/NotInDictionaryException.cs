namespace WordMasterMind.Exceptions;

public class NotInDictionaryException : Exception
{
    public const string MessageText = "not a valid word in the Scrabble dictionary";

    public NotInDictionaryException() : base(message: MessageText)
    {
    }
}
namespace GameEngine.Library.Exceptions;

public class NotInDictionaryException : GameEngineException
{
    public const string MessageText = "Not a valid word in the selected dictionary.";

    public NotInDictionaryException() : base(message: MessageText)
    {
    }
}
namespace WordMasterMind.Exceptions;

public class HardModeException : Exception
{
    public const string MessageText = "You cannot change a letter that is in the correct position.";

    public HardModeException() : base(message: MessageText)
    {
    }
}
namespace GameEngine.Library.Exceptions;

public class InvalidLengthException : GameEngineException
{
    public const string MessageText = "word must be between minLength and maxLength.";
    public readonly int MaxLength;

    public readonly int MinLength;

    public InvalidLengthException(int minLength, int maxLength) : base(message: MessageText)
    {
        this.MinLength = minLength;
        this.MaxLength = maxLength;
    }
}
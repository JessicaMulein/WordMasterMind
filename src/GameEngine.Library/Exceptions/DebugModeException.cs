namespace GameEngine.Library.Exceptions;

public class DebugModeException : GameEngineException
{
    public const string MessageText = "Only available in debug and testing mode.";
    public readonly string ParamName;

    public DebugModeException(string paramName) : base(message: MessageText)
    {
        this.ParamName = paramName;
    }
}
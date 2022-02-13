namespace GameEngine.Library.Exceptions;

public class HardModeLockedException : GameEngineException
{
    public const string MessageText = "Hard Mode cannot be changed after the first attempt.";

    public HardModeLockedException() : base(message: MessageText)
    {
    }
}
using System.Runtime.Serialization;

namespace GameEngine.Library.Exceptions;

/// <summary>
///     Toplevel class so we can check if the exception came from the game engine/is a gameplay issue
/// </summary>
public class GameEngineException : Exception
{
    public GameEngineException()
    {
    }

    protected GameEngineException(SerializationInfo info, StreamingContext context) : base(info: info,
        context: context)
    {
    }

    public GameEngineException(string? message) : base(message: message)
    {
    }

    public GameEngineException(string? message, Exception? innerException) : base(message: message,
        innerException: innerException)
    {
    }
}
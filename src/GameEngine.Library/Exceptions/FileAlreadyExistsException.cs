namespace GameEngine.Library.Exceptions;

public class FileAlreadyExistsException : GameEngineException
{
    public FileAlreadyExistsException(string message) : base(message: message)
    {
    }
}
namespace GameEngine.Library.Exceptions;

public class FileAlreadyExistsException : Exception
{
    public FileAlreadyExistsException(string message) : base(message: message)
    {
    }
}
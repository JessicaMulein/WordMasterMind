namespace GameEngine.Library.Enumerations;

public enum LiteralDictionaryFileType
{
    /// <summary>
    ///     File with one word per line
    /// </summary>
    TextWithNewLines,

    /// <summary>
    ///     File containing a JSON array with a list of words
    /// </summary>
    JsonStringArray,

    /// <summary>
    ///     Binary serialized dictionary created by this library
    /// </summary>
    Binary,
}
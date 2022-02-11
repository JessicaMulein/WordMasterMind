using System.Text;
using WordMasterMind.Blazor.Helpers;
// ReSharper disable MemberCanBePrivate.Global

namespace WordMasterMind.Blazor.Components;

public partial class GameKeyboard
{
    public const char BackspaceKey = WordMasterMind.Library.Helpers.Constants.BackspaceChar;
    public const char EnterKey = WordMasterMind.Library.Helpers.Constants.NewLineChar;
    public const char SkipBlank = WordMasterMind.Library.Helpers.Constants.NullChar;

    private static readonly StringBuilder KeyboardBuffer;

    public static readonly char[,] Keys;

    static GameKeyboard()
    {
        Keys = new char[3, 10]
        {
            {'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P'},
            {'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', SkipBlank},
            {EnterKey, 'Z', 'X', 'C', 'V', 'B', 'N', 'M', BackspaceKey, SkipBlank},
        };
        KeyboardBuffer = new StringBuilder();
    }

    private void OnClick(char keyValue)
    {
        Console.WriteLine(value: keyValue);
    }
}
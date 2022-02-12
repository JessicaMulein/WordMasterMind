using System.Text;
using Microsoft.AspNetCore.Components;
using GameEngine.Blazor.Interfaces;
using GameEngine.Library.Exceptions;
using GameEngine.Library.Helpers;

// ReSharper disable MemberCanBePrivate.Global

namespace GameEngine.Blazor.Components;

public partial class GameKeyboard
{
    public const char BackspaceKey = Constants.BackspaceChar;
    public const char EnterKey = Constants.NewLineChar;
    public const char SkipBlank = Constants.NullChar;

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

#pragma warning disable CS8618
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
#pragma warning restore CS8618

    private void OnClick(char keyValue)
    {
        switch (keyValue)
        {
            case BackspaceKey:
                KeyboardBuffer.Remove(startIndex: KeyboardBuffer.Length - 1,
                    length: 1);
                break;
            case EnterKey:
                var attemptWord = KeyboardBuffer.ToString();
                var wordMasterMindGame = this.GameStateMachine.Game;
                if (wordMasterMindGame is not null)
                    try
                    {
                        wordMasterMindGame.MakeAttempt(wordAttempt: attemptWord);
                        KeyboardBuffer.Clear();
                    }
                    catch (HardModeException _)
                    {
                        // ignored
                    }
                    catch (Exception _)
                    {
                        // ignored
                    }

                break;
            default:
                KeyboardBuffer.Append(value: keyValue);
                break;
        }

        this.GameStateMachine.CurrentAttemptString = KeyboardBuffer.ToString();
    }
}
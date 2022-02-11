using System.Text;
using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Enumerations;
using WordMasterMind.Blazor.Helpers;
using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Blazor.Models;
using WordMasterMind.Library.Exceptions;

// ReSharper disable MemberCanBePrivate.Global

namespace WordMasterMind.Blazor.Components;

public partial class GameKeyboard
{
    public const char BackspaceKey = WordMasterMind.Library.Helpers.Constants.BackspaceChar;
    public const char EnterKey = WordMasterMind.Library.Helpers.Constants.NewLineChar;
    public const char SkipBlank = WordMasterMind.Library.Helpers.Constants.NullChar;

    private static readonly StringBuilder KeyboardBuffer;

    public static readonly char[,] Keys;
    
#pragma warning disable CS8618
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
#pragma warning restore CS8618

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
        switch (keyValue)
        {
            case BackspaceKey:
                KeyboardBuffer.Remove(startIndex: KeyboardBuffer.Length - 1, length: 1);
                break;
            case EnterKey:
                var attemptWord = KeyboardBuffer.ToString();
                var wordMasterMindGame = this.GameStateMachine.Game;
                if (wordMasterMindGame is not null)
                {
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
                }
                break;
            default:
                KeyboardBuffer.Append(value: keyValue);
                break;
        }
    }
}
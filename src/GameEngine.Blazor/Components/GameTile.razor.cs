using Microsoft.AspNetCore.Components;
using GameEngine.Library.Enumerations;

namespace GameEngine.Blazor.Components;

/// <summary>
///     GameTile is used by GameRow/GameBoard and the Rules splash, but notably rules does not use GameRow.
/// </summary>
public partial class GameTile
{
    [ParameterAttribute] public char Letter { get; set; } = GameEngine.Library.Helpers.Constants.EmptyChar;

    [ParameterAttribute] public string LetterString
    {
        get => this.Letter.ToString().ToUpperInvariant();
        set => this.Letter = value.ToUpperInvariant()[index: 0];
    }
    [ParameterAttribute] public LetterEvaluation LetterEvaluation { get; set; } = LetterEvaluation.Tbd;

    [ParameterAttribute]
    public string EvaluationString
    {
        get => Enum.GetName(
            enumType: typeof(LetterEvaluation),
            value: this.LetterEvaluation)!.ToLowerInvariant();
        set => this.LetterEvaluation = (LetterEvaluation)Enum.Parse(
            enumType: typeof(LetterEvaluation),
            value: value.ToLowerInvariant(),
            ignoreCase: true);
    }

    [ParameterAttribute] public bool Reveal { get; set; }
}
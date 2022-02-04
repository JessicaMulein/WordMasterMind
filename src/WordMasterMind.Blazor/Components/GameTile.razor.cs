using Microsoft.AspNetCore.Components;
using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Blazor.Components;

public partial class GameTile
{
    [CascadingParameter] public char Letter { get; set; }

    [CascadingParameter] public string Evaluation { get; set; } = nameof(LetterEvaluation.Absent).ToLowerInvariant();

    public LetterEvaluation LetterEvaluation
    {
        get => (LetterEvaluation) Enum.Parse(
            enumType: typeof(LetterEvaluation),
            value: this.Evaluation,
            ignoreCase: true);
        set => this.Evaluation = value.ToString().ToLowerInvariant();
    }

    [CascadingParameter] public bool Reveal { get; set; }
}
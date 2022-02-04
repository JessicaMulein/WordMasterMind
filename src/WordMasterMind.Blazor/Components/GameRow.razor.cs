using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Library.Helpers;

namespace WordMasterMind.Blazor.Components;

/// <summary>
/// GameRow is  only used by GameBoard which is only visible when GameStateMachine.Game is defined.
/// </summary>
public partial class GameRow
{
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
    [CascadingParameter] public int AttemptNumber { get; set; }
    [CascadingParameter] public string Letters { get; set; } = string.Empty;
    [CascadingParameter] public int Length { get; set; } = Constants.StandardLength;
}
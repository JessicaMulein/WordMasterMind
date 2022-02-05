using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Library.Helpers;

namespace WordMasterMind.Blazor.Components;

/// <summary>
///     GameRow is  only used by GameBoard which is only visible when GameStateMachine.Game is defined.
/// </summary>
public partial class GameRow
{
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
    [ParameterAttribute] public int AttemptNumber { get; set; }
    [ParameterAttribute] public string Letters { get; set; } = string.Empty;
    [ParameterAttribute] public int Length { get; set; } = Constants.StandardLength;
}
using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Interfaces;

namespace WordMasterMind.Blazor.Components;

/// <summary>
///     GameRow is  only used by GameBoard which is only visible when GameStateMachine.Game is defined.
/// </summary>
public partial class GameRow
{
#pragma warning disable CS8618
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
#pragma warning restore CS8618

    [ParameterAttribute] public int AttemptIndex { get; set; } = -1;
}
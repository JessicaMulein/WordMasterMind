using Microsoft.AspNetCore.Components;
using GameEngine.Blazor.Interfaces;

namespace GameEngine.Blazor.Components;

/// <summary>
///     GameBoard is only used/visible while GameStateMachine.Game is defined
/// </summary>
public partial class GameBoard
{
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
}
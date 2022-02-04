using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Interfaces;

namespace WordMasterMind.Blazor.Components;

public partial class GameBoard
{
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
}
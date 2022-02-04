using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Interfaces;

namespace WordMasterMind.Blazor.Components;

public partial class GameSettings
{
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
}
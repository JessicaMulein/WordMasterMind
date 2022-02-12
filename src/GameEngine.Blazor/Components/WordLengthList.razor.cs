using Microsoft.AspNetCore.Components;
using GameEngine.Blazor.Interfaces;

namespace GameEngine.Blazor.Components;

public partial class WordLengthList
{
    public static IEnumerable<int> ValidWordLengths = new int[] { };
#pragma warning disable CS8618
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
#pragma warning restore CS8618
    protected override async Task OnInitializedAsync()
    {
        ValidWordLengths = await this.GameStateMachine.GetDictionaryWordLengths();
    }
}
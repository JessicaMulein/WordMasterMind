using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Interfaces;

namespace WordMasterMind.Blazor.Components;

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
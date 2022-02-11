using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Interfaces;

namespace WordMasterMind.Blazor.Components
{
    public partial class WordLengthList
    {
#pragma warning disable CS8618
        [Inject] public IGameStateMachine GameStateMachine { get; set; }
#pragma warning restore CS8618

        public static IEnumerable<int> ValidWordLengths = new int[] {};
        protected override async Task OnInitializedAsync()
        {
            ValidWordLengths = await GameStateMachine.GetDictionaryWordLengths();
        }
    }
}

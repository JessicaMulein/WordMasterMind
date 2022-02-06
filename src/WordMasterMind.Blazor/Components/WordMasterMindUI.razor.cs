using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Enumerations;
using WordMasterMind.Blazor.Interfaces;

namespace WordMasterMind.Blazor.Components;

// ReSharper disable once InconsistentNaming
public partial class WordMasterMindUI
{
#pragma warning disable CS8618
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
#pragma warning restore CS8618

    private async Task OnRulesDialogClose(bool accepted)
    {
        await Task.Run(action: () =>
        {
            // this will fire events within the state machine setter
            this.GameStateMachine.State = GameState.SourceSelection;
        });
    }

    private async Task OnGameSettingsDialogClose(bool accepted)
    {
        await Task.Run(action: () =>
        {
            // this will fire events within the state machine setter
            this.GameStateMachine.State = GameState.Settings;
        });
    }

    public async Task OnBackClick()
    {
        await Task.Run(action: () => { this.GameStateMachine.State = GameState.Rules; });
    }

    public async Task OnNextClick()
    {
        await Task.Run(action: () =>
        {
            var literalDictionarySourceType = this.GameStateMachine.DictionarySourceType;
            if (literalDictionarySourceType != null)
            {
                this.GameStateMachine.DictionarySourceType = literalDictionarySourceType;
                this.GameStateMachine.State = GameState.LengthSelection;
            }
        });
    }
}
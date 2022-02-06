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

    [Inject] public IHttpClientFactory ClientFactory { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var client = this.ClientFactory.CreateClient(name: "SPAData");
        this.GameStateMachine.HttpClient = client;
    }

    private async Task OnRulesDialogClose(bool accepted)
    {
        // this will fire events within the state machine setter
        await this.GameStateMachine.SetStateAsync(newState: GameState.SourceSelection);
    }

    private async Task OnGameSettingsDialogClose(bool accepted)
    {
        // this will fire events within the state machine setter
        await this.GameStateMachine.SetStateAsync(newState: GameState.Settings);
    }

    public async Task OnBackClick()
    {
        await this.GameStateMachine.SetStateAsync(newState: GameState.Rules);
    }

    public async Task OnNextClick()
    {
        var literalDictionarySourceType = this.GameStateMachine.DictionarySourceType;
        if (literalDictionarySourceType != null)
        {
            this.GameStateMachine.DictionarySourceType = literalDictionarySourceType;
            await this.GameStateMachine.SetStateAsync(newState: GameState.LengthSelection);
        }
    }
}
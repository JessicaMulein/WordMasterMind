using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Enumerations;
using WordMasterMind.Blazor.Helpers;
using WordMasterMind.Blazor.Interfaces;

namespace WordMasterMind.Blazor.Components;

// ReSharper disable once InconsistentNaming
public partial class WordMasterMindUI
{
#pragma warning disable CS8618
    [Inject] public IGameStateMachine GameStateMachine { get; set; }

    [Inject] public IHttpClientFactory ClientFactory { get; set; }
#pragma warning restore CS8618

    protected override async Task OnInitializedAsync()
    {
        var client = this.ClientFactory.CreateClient(name: Constants.SpaHttpClientName);
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
        var currentState = this.GameStateMachine.State;
        await this.GameStateMachine.SetStateAsync(newState: currentState switch
        {
            GameState.SourceSelection => GameState.Rules,
            GameState.LengthSelection => GameState.SourceSelection,
            _ => throw new Exception(message: $"Unexpected state {currentState}"),
        });
    }

    public async Task OnNextClick()
    {
        GameState newState;
        var currentState = this.GameStateMachine.State;
        switch (currentState)
        {
            case GameState.SourceSelection:
                newState = GameState.LengthSelection;
                var literalDictionarySourceType = this.GameStateMachine.DictionarySourceType;
                if (literalDictionarySourceType != null)
                {
                    this.GameStateMachine.DictionarySourceType = literalDictionarySourceType;
                }

                break;
            case GameState.LengthSelection:
                newState = GameState.Playing;
                break;
            default:
                throw new Exception(message: $"Unexpected state {currentState}");
        }

        await this.GameStateMachine.SetStateAsync(newState: newState);
    }
}
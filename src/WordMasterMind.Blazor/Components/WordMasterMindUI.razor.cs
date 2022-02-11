using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WordMasterMind.Blazor.Enumerations;
using WordMasterMind.Blazor.Helpers;
using WordMasterMind.Blazor.Interfaces;

namespace WordMasterMind.Blazor.Components;

// ReSharper disable once InconsistentNaming
public partial class WordMasterMindUI
{
    protected override async Task OnInitializedAsync()
    {
        var client = this.ClientFactory.CreateClient(name: Constants.SpaHttpClientName);
        this.GameStateMachine.HttpClient = client;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task OnBackClick()
    {
        var currentState = this.GameStateMachine.State;
        await this.GameStateMachine.ChangeStateAsync(newState: currentState switch
        {
            GameState.SourceSelection => GameState.Rules,
            GameState.LengthSelection => GameState.SourceSelection,
            _ => throw new Exception(message: $"Unexpected state {currentState}"),
        });
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task OnNextClick()
    {
        GameState newState;
        var currentState = this.GameStateMachine.State;
        if (currentState is GameState.SourceSelection)
        {
            newState = GameState.LengthSelection;
            this.GameStateMachine.DictionarySourceType = this.GameStateMachine.DictionarySourceType;
        }
        else if (currentState is GameState.LengthSelection)
        {
            newState = GameState.Playing;
        }
        else
        {
            throw new Exception(message: $"Unexpected state {currentState}");
        }

        await this.GameStateMachine.ChangeStateAsync(newState: newState);
    }

    private async Task OnRulesClick(MouseEventArgs obj)
    {
        await this.GameStateMachine.ChangeStateAsync(newState: GameState.Rules);
    }

    private async Task OnRulesDialogClose(bool accepted)
    {
        // this will fire events within the state machine setter
        await this.GameStateMachine.ChangeStateAsync(newState: GameState.SourceSelection);
    }

    private async Task OnSettingsClick(MouseEventArgs obj)
    {
        await this.GameStateMachine.ChangeStateAsync(newState: GameState.Settings);
    }

    private async Task OnGameSettingsDialogClose(bool accepted)
    {
        // this will fire events within the state machine setter
        await this.GameStateMachine.ChangeStateAsync(newState: GameState.Settings);
    }
#pragma warning disable CS8618
    [Inject] public IGameStateMachine GameStateMachine { get; set; }

    [Inject] public IHttpClientFactory ClientFactory { get; set; }
#pragma warning restore CS8618
}
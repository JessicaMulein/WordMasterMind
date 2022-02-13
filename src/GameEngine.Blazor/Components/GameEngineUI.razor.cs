using GameEngine.Blazor.Enumerations;
using GameEngine.Blazor.Helpers;
using GameEngine.Blazor.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GameEngine.Blazor.Components;

// ReSharper disable once InconsistentNaming
public partial class GameEngineUI
{
    protected override async Task OnInitializedAsync()
    {
        await Task.Run(action: () =>
        {
            var client = this.ClientFactory.CreateClient(name: Constants.SpaHttpClientName);
            this.GameStateMachine.HttpClient = client;
        });
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task OnBackClick()
    {
        var currentState = this.GameStateMachine.State;
        await UIHelpers.TryChangeGameStateAndAlertOnGameEngineException(
            stateMachine: this.GameStateMachine,
            newState: currentState switch
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

        await UIHelpers.TryChangeGameStateAndAlertOnGameEngineException(
            stateMachine: this.GameStateMachine,
            newState: newState);
    }

    private async Task OnRulesClick(MouseEventArgs obj)
    {
        await UIHelpers.TryChangeGameStateAndAlertOnGameEngineException(
            stateMachine: this.GameStateMachine,
            newState: GameState.Rules);
    }

    private async Task OnRulesDialogClose(bool accepted)
    {
        // this will fire events within the state machine setter
        await UIHelpers.TryChangeGameStateAndAlertOnGameEngineException(
            stateMachine: this.GameStateMachine,
            newState: GameState.SourceSelection);
    }

    private async Task OnSettingsClick(MouseEventArgs obj)
    {
        await UIHelpers.TryChangeGameStateAndAlertOnGameEngineException(
            stateMachine: this.GameStateMachine,
            newState: GameState.Settings);
    }

    private async Task OnGameSettingsDialogClose(bool accepted)
    {
        // this will fire events within the state machine setter
        await UIHelpers.TryPreviousStateAndAlertOnGameEngineException(stateMachine: this.GameStateMachine);
    }
#pragma warning disable CS8618
    [Inject] public IGameStateMachine GameStateMachine { get; set; }

    [Inject] public IHttpClientFactory ClientFactory { get; set; }
#pragma warning restore CS8618
}
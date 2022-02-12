using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using GameEngine.Blazor.Interfaces;
using GameEngine.Blazor.Shared;
using GameEngine.Library.Exceptions;

namespace GameEngine.Blazor.Components;

public partial class GameSettingsDialog
{
#pragma warning disable CS8618
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
    [Inject] public IJSRuntime JsRuntime { get; set; }
#pragma warning restore CS8618

    [Parameter] public EventCallback<bool> OnSettingsClosed { get; set; }

    private async Task ModalCancel()
    {
        try
        {
            await this.GameStateMachine.ChangeStateAsync(newState: this.GameStateMachine.PreviousState);
            if (this.OnSettingsClosed.HasDelegate)
                await this.OnSettingsClosed.InvokeAsync(arg: false);
        }
        catch (Exception _)
        {
            // ignore
        }
    }

    private async Task ModalOk()
    {
        try
        {
            await this.GameStateMachine.ChangeStateAsync(newState: this.GameStateMachine.PreviousState);
            if (this.OnSettingsClosed.HasDelegate)
                await this.OnSettingsClosed.InvokeAsync(arg: true);
        }
        catch (Exception _)
        {
            // ignore
        }
    }

    private void ToggleHardMode()
    {
        try
        {
            var gameActive = this.GameStateMachine.Game is not null;
            // get the current state of the toggle
            var newMode = !(gameActive && this.GameStateMachine.HardMode);
            // try to set the new state in the game
            if (gameActive) this.GameStateMachine.Game!.HardMode = newMode;
            // if that was successful, or no game is active, update the UI
            this.GameStateMachine.HardMode = newMode;
        }
        // ReSharper disable once RedundantNameQualifier
        catch (HardModeLockedException e)
        {
            Console.WriteLine(value: $"Error: {e.Message}");
        }
    }

    private void ToggleDailyWord()
    {
        this.GameStateMachine.DailyWord = !this.GameStateMachine.DailyWord;
    }

    private async Task ToggleNightMode()
    {
        this.GameStateMachine.NightMode = !this.GameStateMachine.NightMode;
        await this.JsRuntime.InvokeVoidAsync(identifier: "window.JsFunctions.setNightMode", this.GameStateMachine.NightMode);
    }
}
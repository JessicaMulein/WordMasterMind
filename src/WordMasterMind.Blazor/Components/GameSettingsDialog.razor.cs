using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Interfaces;

namespace WordMasterMind.Blazor.Components;

public partial class GameSettingsDialog
{
#pragma warning disable CS8618
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
#pragma warning restore CS8618

    [Parameter] public EventCallback<bool> OnSettingsClosed { get; set; }

    private async Task ModalCancel()
    {
        await this.GameStateMachine.ChangeStateAsync(newState: this.GameStateMachine.PreviousState);
        if (this.OnSettingsClosed.HasDelegate)
            await this.OnSettingsClosed.InvokeAsync(arg: false);
    }

    private async Task ModalOk()
    {
        await this.GameStateMachine.ChangeStateAsync(newState: this.GameStateMachine.PreviousState);
        if (this.OnSettingsClosed.HasDelegate)
            await this.OnSettingsClosed.InvokeAsync(arg: true);
    }

    private void ToggleHardMode()
    {
        try
        {
            var gameActive = GameStateMachine.Game is not null;
            // get the current state of the toggle
            var newMode = !(gameActive && GameStateMachine.HardMode);
            // try to set the new state in the game
            if (gameActive)
            {
                GameStateMachine.Game!.HardMode = newMode;
            }
            // if that was successful, or no game is active, update the UI
            GameStateMachine.HardMode = newMode;
        }
        // ReSharper disable once RedundantNameQualifier
        catch (WordMasterMind.Library.Exceptions.HardModeLockedException e)
        {
            Console.WriteLine(value: $"Error: {e.Message}");
        }
    }

    private void ToggleDailyWord()
    {
        GameStateMachine.DailyWord = !GameStateMachine.DailyWord;
    }
}
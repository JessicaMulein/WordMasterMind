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
}
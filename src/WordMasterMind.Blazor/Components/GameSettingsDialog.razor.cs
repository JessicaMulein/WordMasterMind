using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Interfaces;

namespace WordMasterMind.Blazor.Components;

public partial class GameSettingsDialog
{
    [Inject] public IGameStateMachine GameStateMachine { get; set; }

    [Parameter] public EventCallback<bool> OnClose { get; set; }

    private Task ModalCancel()
    {
        return this.OnClose.InvokeAsync(arg: false);
    }

    private Task ModalOk()
    {
        return this.OnClose.InvokeAsync(arg: true);
    }
}
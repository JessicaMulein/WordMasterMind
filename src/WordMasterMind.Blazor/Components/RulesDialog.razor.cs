using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Helpers;

namespace WordMasterMind.Blazor.Components;

public partial class RulesDialog
{
    [Parameter] public EventCallback<bool> OnClose { get; set; }

    private async Task ModalCancel()
    {
        if (this.OnClose.HasDelegate)
            await this.OnClose.InvokeAsync(arg: false);
    }

    private async Task ModalOk()
    {
        if (this.OnClose.HasDelegate)
            await this.OnClose.InvokeAsync(arg: true);
    }
}
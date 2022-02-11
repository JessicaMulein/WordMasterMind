using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Helpers;

namespace WordMasterMind.Blazor.Components;

public partial class RulesDialog
{
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
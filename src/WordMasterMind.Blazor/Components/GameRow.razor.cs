using Microsoft.AspNetCore.Components;
using WordMasterMind.Library.Helpers;

namespace WordMasterMind.Blazor.Components;

public partial class GameRow
{
    [CascadingParameter] public string Letters { get; set; } = string.Empty;
    [CascadingParameter] public int Length { get; set; } = Constants.StandardLength;
}
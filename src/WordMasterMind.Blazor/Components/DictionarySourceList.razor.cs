using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Components;

public partial class DictionarySourceList
{
#pragma warning disable CS8618
    [Inject] public IGameStateMachine GameStateMachine { get; set; }
#pragma warning restore CS8618

    // ReSharper disable once MemberCanBePrivate.Global
    public static IEnumerable<LiteralDictionarySource> Sources => LiteralDictionarySource.Sources;

    /// <summary>
    ///     Translator to let the UI use the DictionarySource's type string as a key.
    ///     UI can get/set the currently selected source by using the
    ///     <see cref="WordMasterMind.Blazor.Models.GameStateMachine.DictionarySourceType" /> property.
    /// </summary>
    private string SourceTypeString
    {
        get => Enum.GetName(
            enumType: typeof(LiteralDictionarySourceType),
            value: this.GameStateMachine.DictionarySourceType)!;
        set => this.GameStateMachine.DictionarySourceType = (LiteralDictionarySourceType) Enum.Parse(
            enumType: typeof(LiteralDictionarySourceType),
            value: value,
            ignoreCase: true);
    }
}
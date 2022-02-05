using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Components;

public partial class DictionarySourceList
{
    private string? _sourceTypeString;

    [Inject] public IGameStateMachine GameStateMachine { get; set; }

    public static IEnumerable<LiteralDictionarySource> Sources => LiteralDictionarySource.Sources;

    private string? SourceTypeString
    {
        get => this._sourceTypeString;
        set
        {
            this._sourceTypeString = value;
            this.GameStateMachine.DictionarySourceType = value is null
                ? LiteralDictionarySourceType.Crossword
                : (LiteralDictionarySourceType) Enum.Parse(
                    enumType: typeof(LiteralDictionarySourceType),
                    value: value,
                    ignoreCase: true);
        }
    }
}
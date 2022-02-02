using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Components;

public partial class DictionarySourceList
{
    [Inject]
    public IGameStateMachine GameStateMachine { get; set; }

    [ParameterAttribute] public string SelectName { get; set; }

    public static IEnumerable<LiteralDictionarySource> Sources => LiteralDictionarySource.Sources;

    private string? _sourceTypeString;
    private string? SourceTypeString
    {
        get
        {
            return _sourceTypeString;
        }
        set
        {
            GameStateMachine.DictionarySourceType = value is null ? LiteralDictionarySourceType.Crossword : (LiteralDictionarySourceType)Enum.Parse(
                enumType: typeof(LiteralDictionarySourceType),
                value: value);
        }
    }
}

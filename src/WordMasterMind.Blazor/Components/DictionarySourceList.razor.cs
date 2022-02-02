using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Components;

public partial class DictionarySourceList
{
    [ParameterAttribute] public string SelectName { get; set; }

    public LiteralDictionarySourceType Type
    {
        get => this.SourceTypeString is null ? LiteralDictionarySourceType.Crossword : (LiteralDictionarySourceType) Enum.Parse(
            enumType: typeof(LiteralDictionarySourceType),
            value: this.SourceTypeString);
    }

    public static IEnumerable<LiteralDictionarySource> Sources => LiteralDictionarySource.Sources;

    public string? SourceTypeString { get; set; }
}

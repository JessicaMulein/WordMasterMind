using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using WordMasterMind.Library.Enumerations;
using WordMasterMind.Library.Models;

namespace WordMasterMind.Blazor.Controllers;

[Microsoft.AspNetCore.Components.Route(template: "[controller]")]
public class DictionaryController : ControllerBase
{
    [HttpGet(template: "{typeString}")]
    public ActionResult<JsonObject> Get(string typeString)
    {
        try
        {
            var sourceType = Enum.Parse<LiteralDictionarySources>(value: typeString);
            var dictionary = LiteralDictionarySource.NewFromSourceType(sourceType: sourceType);
            return new ActionResult<JsonObject>(value: dictionary.ToJson());
        }
        catch (Exception)
        {
            return this.BadRequest();
        }
    }
}
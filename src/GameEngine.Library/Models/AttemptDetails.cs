namespace GameEngine.Library.Models;

public record AttemptDetails
{
    public readonly int AttemptNumber;
    public readonly IEnumerable<AttemptLetterDetail> Details;
    public readonly string Word;

    public AttemptDetails(int attemptNumber, string word, IEnumerable<AttemptLetterDetail> details)
    {
        this.AttemptNumber = attemptNumber;
        var attemptLetterDetails = details as AttemptLetterDetail[] ?? details.ToArray();
        this.Details = attemptLetterDetails;
        this.Word = word.ToUpperInvariant();
    }

    public string AttemptString => string.Join(
        separator: "",
        values: this.Details
            .Select(selector: x => x.Letter));

    public IEnumerator<AttemptLetterDetail> GetEnumerator()
    {
        return this.Details.GetEnumerator();
    }
}
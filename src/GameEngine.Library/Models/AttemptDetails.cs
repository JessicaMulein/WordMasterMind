namespace GameEngine.Library.Models;

public record AttemptDetails
{
    public readonly int AttemptNumber;
    public readonly IEnumerable<AttemptLetterDetail> Details;

    public AttemptDetails(int attemptNumber, IEnumerable<AttemptLetterDetail> details)
    {
        this.AttemptNumber = attemptNumber;
        this.Details = details;
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
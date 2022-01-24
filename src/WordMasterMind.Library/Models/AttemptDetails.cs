namespace WordMasterMind.Library.Models;

public record AttemptDetails
{
    public readonly int AttemptNumber;
    public readonly IEnumerable<AttemptDetail> Details;

    public AttemptDetails(int attemptNumber, IEnumerable<AttemptDetail> details)
    {
        this.AttemptNumber = attemptNumber;
        this.Details = details;
    }

    public IEnumerator<AttemptDetail> GetEnumerator()
    {
        return this.Details.GetEnumerator();
    }
}
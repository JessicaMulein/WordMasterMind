namespace WordMasterMind.Library.Models;

public record AttemptDetails
{
    public readonly int AttemptNumber;
    public readonly IEnumerable<AttemptLetterDetail> Details;

    public AttemptDetails(int attemptNumber, IEnumerable<AttemptLetterDetail> details)
    {
        this.AttemptNumber = attemptNumber;
        this.Details = details;
    }

    public IEnumerator<AttemptLetterDetail> GetEnumerator()
    {
        return this.Details.GetEnumerator();
    }
}
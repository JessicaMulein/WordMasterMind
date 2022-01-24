using System.Collections;

namespace WordMasterMind.Library.Models;

public record AttemptDetails : IEnumerable<AttemptDetail>
{
    public readonly IEnumerable<AttemptDetail> Details;

    public AttemptDetails(IEnumerable<AttemptDetail> details)
    {
        this.Details = details;
    }

    public IEnumerator<AttemptDetail> GetEnumerator()
    {
        return this.Details.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.Details.GetEnumerator();
    }
}
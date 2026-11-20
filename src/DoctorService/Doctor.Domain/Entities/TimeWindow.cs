namespace DoctorService.Domain.Entities;

// A continuous range of time within one day
public record TimeWindow(TimeOnly Start, TimeOnly End)
{
    // Removes [cutStart, cutEnd) from this window; returns 0, 1 or 2 pieces
    public IEnumerable<TimeWindow> Subtract(TimeOnly cutStart, TimeOnly cutEnd)
    {
        if (cutEnd <= Start || cutStart >= End)
        {
            yield return this;
            yield break;
        }

        if (cutStart > Start)
            yield return new TimeWindow(Start, cutStart);

        if (cutEnd < End)
            yield return new TimeWindow(cutEnd, End);
    }
}

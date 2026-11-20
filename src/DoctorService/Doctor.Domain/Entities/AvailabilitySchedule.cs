namespace DoctorService.Domain.Entities;

// One block of weekly working hours, e.g. Monday 08:00–12:00
public class AvailabilitySchedule
{
    public Guid Id { get; private set; }
    public Guid DoctorId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set;}
    public bool IsActive { get; private set; }


    private AvailabilitySchedule() { }

    // Id is left empty on purpose: EF generates it when the entity is added through
    // the doctor's collection. A preset Id would make EF treat it as an existing row.
    public static AvailabilitySchedule Create(Guid doctorId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be earlier than end time");

        return new AvailabilitySchedule
        {
            DoctorId = doctorId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            IsActive = true
        };
    }


    public void Deactivate () => IsActive = false;

}

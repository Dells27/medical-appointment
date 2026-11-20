namespace AppointmentService.Domain.Entities
{
    public enum AppointmentStatus
    {
        Scheduled,
        Confirmed,
        Completed,
        Cancelled,
        NoShow
    }


    public class Appointment
    {

        public Guid Id { get; private set; }
        public Guid patientId { get; private set; }
        public Guid doctorId { get; private set; }

        // Copied from the patient's token when booking, so the doctor sees who is coming
        // and notifications can be sent without querying the Patient Service.
        public string? patientName { get; private set; }
        public string? patientEmail { get; private set; }

        public DateTime appointmentDate { get; private set; }
        public TimeOnly appointmentTime { get; private set; }
        public AppointmentStatus status { get; private set; }
        public string? reason { get; private set; }
        public string? cancellationReason { get; private set; }
        public string? cancelledByRole { get; private set; }
        public DateTime? rescheduledAt { get; private set; }
        public DateTime createdAt { get; private set; }
        public DateTime? updatedAt { get; private set; }


        private Appointment() { }

        public static Appointment Create (Guid PatientId, Guid DoctorId, DateTime AppointmentDate, TimeOnly AppointmentTime,
            string? Reason, string? PatientName = null, string? PatientEmail = null)
        {
            return new Appointment {
                Id = Guid.NewGuid(),
                patientId = PatientId,
                doctorId = DoctorId,
                patientName = PatientName,
                patientEmail = PatientEmail,
                appointmentDate = AppointmentDate,
                appointmentTime = AppointmentTime,
                reason = string.IsNullOrWhiteSpace(Reason) ? null : Reason.Trim(),
                createdAt = DateTime.UtcNow
            };
        }

        // True when the user is the patient or the doctor of this appointment
        public bool BelongsTo(Guid userId) => patientId == userId || doctorId == userId;

        public bool IsPatient(Guid userId) => patientId == userId;

        public bool IsDoctor(Guid userId) => doctorId == userId;

        public bool IsActive => status is AppointmentStatus.Scheduled or AppointmentStatus.Confirmed;


        public void Confirm()
        {
            if (status != AppointmentStatus.Scheduled)
                throw new InvalidOperationException("Only scheduled appointments can be confirmed");

            status = AppointmentStatus.Confirmed;
            updatedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            if (!IsActive)
                throw new InvalidOperationException("This appointment cannot be completed");

            status = AppointmentStatus.Completed;
            updatedAt = DateTime.UtcNow;
        }

        public void Cancel (string reason, string? byRole = null)
        {
            if (status == AppointmentStatus.Completed)
                throw new InvalidOperationException("A completed appointment cannot be cancelled");
            if (status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("This appointment is already cancelled");

            status = AppointmentStatus.Cancelled;
            cancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            cancelledByRole = byRole;
            updatedAt = DateTime.UtcNow;
        }

        // The doctor moves the appointment. Since the doctor picked the new time, it is confirmed.
        public void Reschedule(DateTime newDate, TimeOnly newTime)
        {
            if (!IsActive)
                throw new InvalidOperationException("Only scheduled or confirmed appointments can be rescheduled");
            if (newDate.Date == appointmentDate.Date && newTime == appointmentTime)
                throw new InvalidOperationException("The new date and time are the same as the current ones");

            appointmentDate = newDate;
            appointmentTime = newTime;
            status = AppointmentStatus.Confirmed;
            rescheduledAt = DateTime.UtcNow;
            updatedAt = DateTime.UtcNow;
        }

        public void MarkAsNoShow()
        {
            if (!IsActive)
                throw new InvalidOperationException("Only scheduled or confirmed appointments can be marked as no-show");

            status = AppointmentStatus.NoShow;
            updatedAt = DateTime.UtcNow;
        }


    }
}

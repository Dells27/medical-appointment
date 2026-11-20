namespace MedicalRecordService.Domain.Entities;

// The patient's medical background (antecedentes). One per patient.
// Only doctors who have attended the patient can edit it; the patient can read it.
public class MedicalBackground
{
    public Guid Id { get; private set; }
    public Guid patientId { get; private set; }          // auth user id of the patient
    public string? chronicConditions { get; private set; }
    public string? surgeries { get; private set; }
    public string? currentMedications { get; private set; }
    public string? familyHistory { get; private set; }
    public string? allergies { get; private set; }
    public string? habits { get; private set; }          // smoking, alcohol, exercise...
    public string? notes { get; private set; }
    public Guid updatedByDoctorId { get; private set; }
    public DateTime createdAt { get; private set; }
    public DateTime updatedAt { get; private set; }

    private MedicalBackground() { }

    public static MedicalBackground Create(Guid patientId, Guid doctorId)
    {
        return new MedicalBackground
        {
            Id = Guid.NewGuid(),
            patientId = patientId,
            updatedByDoctorId = doctorId,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow
        };
    }

    public void Update(Guid doctorId, string? chronicConditions, string? surgeries, string? currentMedications,
        string? familyHistory, string? allergies, string? habits, string? notes)
    {
        this.chronicConditions = Clean(chronicConditions);
        this.surgeries = Clean(surgeries);
        this.currentMedications = Clean(currentMedications);
        this.familyHistory = Clean(familyHistory);
        this.allergies = Clean(allergies);
        this.habits = Clean(habits);
        this.notes = Clean(notes);
        updatedByDoctorId = doctorId;
        updatedAt = DateTime.UtcNow;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

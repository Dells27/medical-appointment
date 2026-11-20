using MedicalRecordService.Application.DTOs;
using MedicalRecordService.Application.Interfaces;
using MedicalRecordService.Application.UseCases.FillNotes;
using MedicalRecordService.Domain.Entities;

namespace MedicalRecordService.Application.UseCases.PatientClinicalData;

// Medical background and consultation history, as seen by the patient or by a doctor.
// A doctor only gets access to a patient they have (or had) an appointment with.
public class PatientClinicalDataHandler
{
    private readonly IMedicalRecordRepository _repository;
    private readonly IAppointmentServiceClient _appointments;

    public PatientClinicalDataHandler(IMedicalRecordRepository repository, IAppointmentServiceClient appointments)
    {
        _repository = repository;
        _appointments = appointments;
    }

    // ---------- Medical background ----------

    public async Task<MedicalBackgroundResponse> GetMyBackground(Guid patientUserId)
    {
        var background = await _repository.GetBackgroundAsync(patientUserId);
        return MapBackground(patientUserId, background);
    }

    public async Task<MedicalBackgroundResponse> GetBackgroundForDoctor(Guid patientUserId)
    {
        await EnsureDoctorCanAccess(patientUserId);
        var background = await _repository.GetBackgroundAsync(patientUserId);
        return MapBackground(patientUserId, background);
    }

    public async Task<MedicalBackgroundResponse> UpdateBackground(Guid doctorUserId, Guid patientUserId, UpdateMedicalBackgroundRequest request)
    {
        await EnsureDoctorCanAccess(patientUserId);

        var background = await _repository.GetBackgroundAsync(patientUserId);
        if (background is null)
        {
            background = MedicalBackground.Create(patientUserId, doctorUserId);
            await _repository.AddBackgroundAsync(background);
        }

        background.Update(doctorUserId, request.chronicConditions, request.surgeries, request.currentMedications,
            request.familyHistory, request.allergies, request.habits, request.notes);

        await _repository.SaveChangesAsync();
        return MapBackground(patientUserId, background);
    }

    // ---------- Consultation history ----------

    // All consultations of a patient (with any doctor), so the doctor can prepare the visit
    public async Task<List<MedicalRecordResponse>> GetPatientHistoryForDoctor(Guid patientUserId)
    {
        await EnsureDoctorCanAccess(patientUserId);
        var records = await _repository.GetByPatientIdAsync(patientUserId);
        return records.Select(FillNotesHandler.MapToResponse).ToList();
    }

    // Consultations the doctor attended
    public async Task<List<MedicalRecordResponse>> GetDoctorConsultations(Guid doctorUserId)
    {
        var records = await _repository.GetByDoctorIdAsync(doctorUserId);
        return records.Select(FillNotesHandler.MapToResponse).ToList();
    }

    private async Task EnsureDoctorCanAccess(Guid patientUserId)
    {
        if (!await _appointments.HasRelationshipAsync(patientUserId))
            throw new UnauthorizedAccessException("You can only access patients who have appointments with you");
    }

    private static MedicalBackgroundResponse MapBackground(Guid patientUserId, MedicalBackground? b) => new()
    {
        patientId = patientUserId,
        chronicConditions = b?.chronicConditions,
        surgeries = b?.surgeries,
        currentMedications = b?.currentMedications,
        familyHistory = b?.familyHistory,
        allergies = b?.allergies,
        habits = b?.habits,
        notes = b?.notes,
        updatedByDoctorId = b?.updatedByDoctorId,
        updatedAt = b?.updatedAt
    };
}

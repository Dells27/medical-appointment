using MedicalRecordService.Application.DTOs;
using MedicalRecordService.Application.Interfaces;
using MedicalRecordService.Application.UseCases.FillNotes;

namespace MedicalRecordService.Application.UseCases.GetMedicalRecords;

public class GetMedicalRecordsHandler
{
    private readonly IMedicalRecordRepository _repository;

    public GetMedicalRecordsHandler(IMedicalRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MedicalRecordResponse>> HandleByPatient(Guid patientId)
    {
        var records = await _repository.GetByPatientIdAsync(patientId);
        return records.Select(r => FillNotesHandler.MapToResponse(r)).ToList();
    }

    // Only the patient of the record or the doctor who wrote it can read it
    public async Task<MedicalRecordResponse> HandleById(Guid id, Guid userId)
    {
        var record = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Medical record not found");

        if (record.patientId != userId && record.doctorId != userId)
            throw new UnauthorizedAccessException("You cannot access this medical record");

        return FillNotesHandler.MapToResponse(record);
    }

    // Only the doctor who attended the appointment can read its record this way
    public async Task<MedicalRecordResponse> HandleByAppointment(Guid appointmentId, Guid doctorId)
    {
        var record = await _repository.GetByAppointmentIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Medical record not found");

        if (record.doctorId != doctorId)
            throw new UnauthorizedAccessException("You cannot access this medical record");

        return FillNotesHandler.MapToResponse(record);
    }
}

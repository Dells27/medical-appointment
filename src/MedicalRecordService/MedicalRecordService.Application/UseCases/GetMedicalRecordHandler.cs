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

    public async Task<MedicalRecordResponse> HandleById(Guid id)
    {
        var record = await _repository.GetByIdAsync(id);
        if (record is null)
            throw new Exception("Registro clínico no encontrado");

        return FillNotesHandler.MapToResponse(record);
    }

    public async Task<MedicalRecordResponse> HandleByAppointment(Guid appointmentId, Guid doctorId)
    {
        var record = await _repository.GetByAppointmentIdAsync(appointmentId);
        if (record is null)
            throw new KeyNotFoundException("Medical record not found");

        // El registro solo lo puede ver el médico que atendió la cita
        if (record.doctorId != doctorId)
            throw new UnauthorizedAccessException("You cannot access this record");

        return FillNotesHandler.MapToResponse(record);
    }
}



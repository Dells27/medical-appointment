using MedicalRecordService.Application.DTOs;
using MedicalRecordService.Application.Interfaces;

namespace MedicalRecordService.Application.UseCases.FillNotes;

public class FillNotesHandler
{
    private readonly IMedicalRecordRepository _repository;

    public FillNotesHandler(IMedicalRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<MedicalRecordResponse> Handle(Guid recordId, FillNotesRequest request)
    {
        var record = await _repository.GetByIdAsync(recordId);
        if (record is null)
            throw new Exception("Registro clínico no encontrado");

        record.FillNotes(request.diagnosis, request.Treatment, request.observations);
        await _repository.SaveChangesAsync();

        return MapToResponse(record);
    }

    public static MedicalRecordResponse MapToResponse(Domain.Entities.MedicalRecord record)
    {
        return new MedicalRecordResponse
        {
            Id = record.Id,
            appointmentId = record.appointmentId,
            patientId = record.patientId,
            doctorId = record.doctorId,
            appointmentDate = record.appointmentDate,
            diagnosis = record.diagnosis,
            treatment = record.treatment,
            observations = record.observations,
            isCompleted = record.isCompleted,
            createdAt = record.createdAt
        };
    }
}
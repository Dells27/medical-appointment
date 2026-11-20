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

    // Only the doctor who attended the appointment can write its clinical notes
    public async Task<MedicalRecordResponse> Handle(Guid recordId, FillNotesRequest request, Guid doctorUserId)
    {
        var record = await _repository.GetByIdAsync(recordId)
            ?? throw new KeyNotFoundException("Medical record not found");

        if (record.doctorId != doctorUserId)
            throw new UnauthorizedAccessException("You cannot edit this medical record");

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

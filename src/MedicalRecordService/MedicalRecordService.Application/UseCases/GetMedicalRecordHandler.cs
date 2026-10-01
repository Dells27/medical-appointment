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
}
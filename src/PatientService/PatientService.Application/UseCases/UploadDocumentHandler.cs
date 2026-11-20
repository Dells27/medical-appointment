using PatientService.Application.DTOs;
using PatientService.Application.Interfaces;
using PatientService.Domain.Entities;

namespace PatientService.Application.UseCases.UploadDocument;

// Medical documents of a patient: the ones the patient shares and the ones doctors issue.
// Doctors can only read or upload documents of patients they have appointments with.
public class UploadDocumentHandler
{
    private readonly IPatientRepository _patientRepository;
    private readonly IS3Service _s3Service;
    private readonly IAppointmentServiceClient _appointments;

    public UploadDocumentHandler(IPatientRepository patientRepository, IS3Service s3Service, IAppointmentServiceClient appointments)
    {
        _patientRepository = patientRepository;
        _s3Service = s3Service;
        _appointments = appointments;
    }

    public async Task<MedicalDocumentResponse> Upload(Guid patientUserId, DocumentUploader uploader, UploadDocumentCommand command)
    {
        if (uploader.Role == "Doctor")
            await EnsureDoctorCanAccess(patientUserId);
        else if (uploader.UserId != patientUserId)
            throw new UnauthorizedAccessException("You can only upload your own documents");

        MedicalDocument.Validate(uploader.Role, command.DocumentType, command.ContentType, command.FileSize);

        var patient = await _patientRepository.GetByUserIdAsync(patientUserId)
            ?? throw new KeyNotFoundException("Patient profile not found");

        var fileKey = await _s3Service.UploadFileAsync(command.FileStream, command.FileName, command.ContentType);

        var document = MedicalDocument.Create(
            patient.id,
            command.FileName,
            fileKey,
            command.ContentType,
            command.FileSize,
            command.DocumentType,
            uploader.Role,
            uploader.UserId,
            uploader.Name,
            command.AppointmentId,
            command.Description);

        patient.AddDocument(document);
        await _patientRepository.SaveChangesAsync();

        return await MapAsync(document);
    }

    public async Task<List<MedicalDocumentResponse>> GetForPatient(Guid patientUserId)
    {
        var patient = await _patientRepository.GetByUserIdAsync(patientUserId)
            ?? throw new KeyNotFoundException("Patient profile not found");

        var documents = new List<MedicalDocumentResponse>();
        foreach (var document in patient.Document.OrderByDescending(d => d.uploadedAt))
            documents.Add(await MapAsync(document));

        return documents;
    }

    public async Task<List<MedicalDocumentResponse>> GetForDoctor(Guid patientUserId)
    {
        await EnsureDoctorCanAccess(patientUserId);
        return await GetForPatient(patientUserId);
    }

    public async Task EnsureDoctorCanAccess(Guid patientUserId)
    {
        if (!await _appointments.HasRelationshipAsync(patientUserId))
            throw new UnauthorizedAccessException("You can only access patients who have appointments with you");
    }

    private async Task<MedicalDocumentResponse> MapAsync(MedicalDocument d) => new()
    {
        Id = d.id,
        fileName = d.fileName,
        fileType = d.fileType,
        documentType = d.documentType,
        fileSize = d.fileSize,
        uploadedByRole = d.uploadedByRole,
        uploadedByName = d.uploadedByName,
        appointmentId = d.appointmentId,
        description = d.description,
        UploadedAt = d.uploadedAt,
        DownloadUrl = await _s3Service.GeneratePresignedUrlAsync(d.fileKey) // valid for 1 hour
    };
}

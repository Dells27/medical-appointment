using PatientService.Application.DTOs;
using PatientService.Application.Interfaces;
using PatientService.Domain.Entities;

namespace PatientService.Application.UseCases
{
    public class CreatePatientHandler
    {
        private readonly IPatientRepository _patientRepository;


        public CreatePatientHandler (IPatientRepository repository)
        {
            _patientRepository = repository;
        }

        public async Task<PatientResponse> Handle(CreatePatientRequest request)
        {
            var exists = await _patientRepository.ExistsAsync(request.userId);
            if (exists)
                throw new InvalidOperationException("This user already has a patient profile");

            if (string.IsNullOrWhiteSpace(request.name))
                throw new ArgumentException("Name is required");

            var patient = Patient.Create(
                request.userId,
                request.name.Trim(),
                request.email,
                request.dateOfBirth,
                request.phoneNumber,
                request.bloodType,
                request.allergies
                );

            await _patientRepository.AddAsync( patient );
            await _patientRepository.SaveChangesAsync();

            return MapToResponse(patient);
        }

        public async Task<PatientResponse> GetByUserIdAsync(Guid userId)
        {
            var patient = await GetPatientOrThrow(userId);
            return MapToResponse(patient);
        }

        public async Task<PatientResponse> UpdateAsync(Guid userId, UpdatePatientRequest request)
        {
            var patient = await GetPatientOrThrow(userId);

            patient.UpdateProfile(
                request.name,
                request.phoneNumber,
                DateTime.SpecifyKind(request.dateOfBirth.Date, DateTimeKind.Utc),
                request.bloodType,
                request.allergies);

            await _patientRepository.SaveChangesAsync();
            return MapToResponse(patient);
        }

        private async Task<Patient> GetPatientOrThrow(Guid userId)
        {
            return await _patientRepository.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Patient profile not found");
        }


        public static PatientResponse MapToResponse(Patient patient)
        {
            return new PatientResponse {
                Id = patient.id,
                userId = patient.userId,
                name = patient.name,
                email = patient.email,
                dateOfBirth = patient.dateOfBirth,
                phoneNumber = patient.phoneNumber,
                bloodType = patient.bloodType,
                allergies = patient.allergies,
                Documents = patient.Document.Select(d => new MedicalDocumentResponse
                {
                    Id = d.id,
                    fileName = d.fileName,
                    fileSize = d.fileSize,
                    fileType = d.fileType,
                    documentType = d.documentType,
                    UploadedAt= d.uploadedAt

                }).ToList()
            };
        }

        // A doctor reads the profile of a patient they have appointments with
        public async Task<PatientResponse> GetForDoctor(Guid patientUserId, IAppointmentServiceClient appointments)
        {
            if (!await appointments.HasRelationshipAsync(patientUserId))
                throw new UnauthorizedAccessException("You can only access patients who have appointments with you");

            var response = MapToResponse(await GetPatientOrThrow(patientUserId));
            response.Documents = new(); // documents have their own endpoint with download links
            return response;
        }


    }
}

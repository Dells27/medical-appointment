using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                throw new Exception("Este usuario ya tiene un perfil de paciente");


            var patient = Patient.Create(
                request.userId,
                request.name,
                request.email,
                request.dateOfBirth,
                request.phoneNumber,
                request.bloodType,
                request.allergies
                );


            await _patientRepository.AddAsync( patient );
            await _patientRepository.SaveChangesAsync();

            return MapToResponse(patient);
;        }

        public async Task<PatientResponse> GetByUserIdAsync(Guid userId)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient is null)
                throw new Exception("Perfil de paciente no encontrado");

            return MapToResponse(patient);
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

        public async Task<List<MedicalDocumentResponse>> GetDocuments(Guid userId, IS3Service s3Service)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            if (patient is null)
                throw new Exception("Perfil de paciente no encontrado");

            var documents = new List<MedicalDocumentResponse>();

            foreach (var doc in patient.Document)
            {
                var downloadUrl = await s3Service.GeneratePresignedUrlAsync(doc.fileKey);

                documents.Add(new MedicalDocumentResponse
                {
                    Id = doc.id,
                    fileName = doc.fileName,
                    fileType = doc.fileType,
                    documentType = doc.documentType,
                    fileSize = doc.fileSize,
                    UploadedAt = doc.uploadedAt,
                    DownloadUrl = downloadUrl
                });

            }
            return documents;
        }


    }
}

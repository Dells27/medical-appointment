using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace PatientService.Application.DTOs
{
    // Request para crear perfil de paciente

    public class CreatePatientRequest
    {
        [JsonIgnore]
        public Guid userId { get; set; }

        public string name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public DateTime dateOfBirth { get; set; }
        public string phoneNumber { get; set; } = string.Empty;
        public string bloodType { get; set; } = string.Empty;
        public string? allergies { get; set; }
    }

    // Request para actualizar perfil
    public class UpdatePatientRequest
    {
        public string name { get; set; } = string.Empty;
        public string phoneNumber { get; set; } = string.Empty;
        public string? allergies { get; set; }
    }


    // Response del paciente
    public class PatientResponse
    {
        public Guid Id { get; set; }
        public Guid userId { get; set; }
        public string name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public DateTime dateOfBirth { get; set; }
        public string phoneNumber { get; set; } = string.Empty;
        public string bloodType { get; set; } = string.Empty;
        public string? allergies { get; set; }
        public List<MedicalDocumentResponse> Documents { get; set; } = new();
    }


    // Response del documento médico
    public class MedicalDocumentResponse
    {
        public Guid Id { get; set; }
        public string fileName { get; set; } = string.Empty;
        public string fileType { get; set; } = string.Empty;
        public string documentType { get; set; } = string.Empty;
        public long fileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? DownloadUrl { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientService.Domain.Entities
{
    public class Patient
    {
        public Guid id { get; private set; }
        public Guid userId { get; private set; }
        public string name { get; private set; }
        public string email { get; private set; }
        public DateTime dateOfBirth { get; private set; }
        public string phoneNumber { get; private set; }
        public string bloodType { get; private set; }
        public string? allergies { get; private set; }
        public bool isActive { get; private set; }
        public DateTime createdAt { get; private set; }

        private readonly List<MedicalDocument> _documents = new();
        public IReadOnlyCollection<MedicalDocument> Document => _documents.AsReadOnly();


        private Patient() { }

        public static Patient Create(Guid UserId, string Name, string Email, DateTime DateOfBirth, string PhoneNumber, string BloodType, string? Allergies)
        {

            return new Patient {
                id = Guid.NewGuid(),
                userId = UserId,
                name = Name,
                email = Email,
                dateOfBirth = DateOfBirth,
                phoneNumber = PhoneNumber,
                bloodType = BloodType,
                allergies = Allergies,
                isActive = true,
                createdAt = DateTime.UtcNow
            };
        }


        public void UpdateProfile(string Name, string PhoneNumber, DateTime DateOfBirth, string BloodType, string? Allergies)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Name is required");
            if (DateOfBirth.Date > DateTime.UtcNow.Date)
                throw new ArgumentException("Date of birth cannot be in the future");

            name = Name.Trim();
            phoneNumber = PhoneNumber.Trim();
            dateOfBirth = DateOfBirth;
            bloodType = BloodType;
            allergies = string.IsNullOrWhiteSpace(Allergies) ? null : Allergies.Trim();
        }

        public void AddDocument(MedicalDocument document)
        {
            _documents.Add(document);
        }

    }


}





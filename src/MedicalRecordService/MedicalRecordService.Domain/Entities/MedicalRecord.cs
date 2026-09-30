using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MedicalRecordService.Domain.Entities
{
    public class MedicalRecord
    {

        public Guid Id { get; private set; }
        public Guid appointmentId { get; private set; }
        public Guid patientId { get; private set; }
        public Guid doctorId { get; private set; }
        public DateTime appointmentDate { get; private set; }
        public string? diagnosis { get; private set; }
        public string? treatment { get; private set; }
        public string? observations { get; private set; }
        public bool isCompleted { get; private set; }
        public DateTime createdAt { get; private set; }
        public DateTime? updatedAt { get; private set; }



        private MedicalRecord() { }

        public static MedicalRecord CreateEmpty(Guid appointmentId, Guid patientId, Guid doctorId, DateTime appointmentDate)
        {
            return new MedicalRecord
            {
                Id = Guid.NewGuid(),
                appointmentId = appointmentId,
                patientId = patientId,
                doctorId = doctorId,
                appointmentDate = appointmentDate,
                isCompleted = false,
                createdAt = DateTime.UtcNow
            };
        }

        public void FillNotes(string Diagnosis, string Treatment, string? Observations)
        {
            if (isCompleted)
                throw new Exception("Este registro clínico ya fue completado");

            diagnosis = Diagnosis;
            treatment = Treatment;
            observations = Observations;
            isCompleted = true;
            updatedAt = DateTime.UtcNow;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedicalRecordService.Domain.Entities;

namespace MedicalRecordService.Application.Interfaces
{
    public interface IMedicalRecordRepository
    {
        Task<MedicalRecord?> GetByIdAsync(Guid Id);
        Task<MedicalRecord?> GetByAppointmentIdAsync(Guid id);
        Task<List<MedicalRecord>> GetByPatientIdAsync(Guid patientId);
        Task AddAsync(MedicalRecord record);
        Task SaveChangesAsync();
    }
}

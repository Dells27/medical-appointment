using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedicalRecordService.Application.Interfaces;
using MedicalRecordService.Application.DTOs;
using MedicalRecordService.Domain.Entities;

namespace MedicalRecordService.Application.UseCases
{
    public class CreateEmptyRecordHandler
    {

        private readonly IMedicalRecordRepository _medicalRecordRepository;

        public CreateEmptyRecordHandler (IMedicalRecordRepository medicalRecordRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
        }


        public async Task Handle(AppointmentCompletedEvent appointmentCompletedEvent)
        {
            var existing = await _medicalRecordRepository.GetByAppointmentIdAsync(appointmentCompletedEvent.appointmentId);
            if (existing is not null)
                return;


            var record = MedicalRecord.CreateEmpty(
                appointmentCompletedEvent.appointmentId,
                appointmentCompletedEvent.patientId,
                appointmentCompletedEvent.doctorId,
                appointmentCompletedEvent.appointmentDate
                );

            await _medicalRecordRepository.AddAsync(record);
            await _medicalRecordRepository.SaveChangesAsync();
        }

    }
}

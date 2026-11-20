using DoctorService.Domain.Entities;
using DoctorService.Application.DTOs;
using DoctorService.Application.Interfaces;


namespace DoctorService.Application.UseCases.CreateDoctor
{
    public class CreateDoctorHanlder
    {
        private readonly IDoctorRepository _repository;
        public CreateDoctorHanlder(IDoctorRepository repository)
        {
           _repository = repository;
        }

        public async Task<DoctorResponse> Handle(CreateDoctorRequest request)
        {
            var exists = await _repository.ExistsAsync(request.userId);
            if (exists)
                throw new InvalidOperationException("This user already has a doctor profile");

            var doctor = Doctor.Create(
                request.userId,
                request.name,
                request.Email,
                request.Specialty,
                request.LicenseNumber
                );

            await _repository.AddAsync( doctor );
            await _repository.SaveChangesAsync();
            return MapToResponse( doctor );
        }

        public static DoctorResponse MapToResponse(Doctor doctor)
        {
            return new DoctorResponse
            {
                Id = doctor.Id,
                UserId = doctor.UserId,
                Name = doctor.Name,
                Email = doctor.Email,
                Specialty = doctor.Specialty,
                LicenseNumber = doctor.LicenseNumber,
                PhotoUrl= doctor.PhotoUrl,
                Bio= doctor.Bio,
                Schedules = MapSchedules(doctor),
            };
        }

        public static PublicDoctorResponse MapToPublicResponse(Doctor doctor)
        {
            return new PublicDoctorResponse
            {
                UserId = doctor.UserId,
                Name = doctor.Name,
                Specialty = doctor.Specialty,
                PhotoUrl = doctor.PhotoUrl,
                Bio = doctor.Bio,
                Schedules = MapSchedules(doctor),
            };
        }

        public static List<ScheduleResponse> MapSchedules(Doctor doctor)
        {
            return doctor.Schedules
                .Where(s => s.IsActive)
                .OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime)
                .Select(s => new ScheduleResponse
                {
                    Id = s.Id,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                }).ToList();
        }

    }
}

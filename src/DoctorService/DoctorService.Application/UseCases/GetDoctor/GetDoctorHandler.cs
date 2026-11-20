using DoctorService.Application.DTOs;
using DoctorService.Application.Interfaces;
using DoctorService.Application.UseCases.CreateDoctor;

namespace DoctorService.Application.UseCases.GetDoctor
{
    public class GetDoctorHandler
    {
        private readonly IDoctorRepository _repository;

        public GetDoctorHandler(IDoctorRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DoctorResponse>> HandleGetAll()
        {
            var doctors = await _repository.GetAllAsync();
            return doctors.Select(CreateDoctorHanlder.MapToResponse).ToList();
        }

        public async Task<DoctorResponse> HandleGetById(Guid id)
        {
            var doctor = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Doctor not found");

            return CreateDoctorHanlder.MapToResponse(doctor);
        }

        // Lookup by the doctor's auth user id (the id stored in appointments)
        public async Task<DoctorResponse> HandleGetByUserId(Guid userId)
        {
            var doctor = await _repository.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Doctor not found");

            return CreateDoctorHanlder.MapToResponse(doctor);
        }

        public async Task<List<DoctorResponse>> HandleGetBySpecialty(string specialty)
        {
            var doctors = await _repository.GetBySpecialtyAsync(specialty);
            return doctors.Select(CreateDoctorHanlder.MapToResponse).ToList();
        }

        public async Task<List<DoctorResponse>> HandleSearch(string? specialty, string? search)
        {
            var doctors = await _repository.SearchAsync(specialty, search);
            return doctors.Select(CreateDoctorHanlder.MapToResponse).ToList();
        }

        // ---------- Public (anonymous) ----------

        public async Task<List<PublicDoctorResponse>> HandlePublicSearch(string? specialty, string? search)
        {
            var doctors = await _repository.SearchAsync(specialty, search);
            return doctors.Select(CreateDoctorHanlder.MapToPublicResponse).ToList();
        }

        public async Task<PublicDoctorResponse> HandlePublicGetByUserId(Guid userId)
        {
            var doctor = await _repository.GetByUserIdAsync(userId);
            if (doctor is null || !doctor.IsActive)
                throw new KeyNotFoundException("Doctor not found");

            return CreateDoctorHanlder.MapToPublicResponse(doctor);
        }

        public async Task<List<SpecialtyResponse>> HandleGetSpecialties()
        {
            var specialties = await _repository.GetSpecialtiesAsync();
            return specialties
                .Select(s => new SpecialtyResponse { Name = s.Specialty, DoctorCount = s.DoctorCount })
                .ToList();
        }
    }
}

using SimpleMedWayDemo.Models;

namespace SimpleMedWayDemo.Services;

public class HospitalService
{
    private readonly List<Hospital> _hospitals = new();

    public Task<List<Hospital>> GetAllHospitalsAsync()
    {
        return Task.FromResult(_hospitals.ToList());
    }

    public Task<Hospital?> GetHospitalByIdAsync(Guid id)
    {
        var hospital = _hospitals.FirstOrDefault(h => h.Id == id);
        return Task.FromResult(hospital);
    }

    public Task<bool> RegisterHospitalAsync(Hospital hospital)
    {
        if (hospital == null)
            return Task.FromResult(false);

        // Check if hospital with same registration number already exists
        if (_hospitals.Any(h => h.RegistrationNumber == hospital.RegistrationNumber))
            return Task.FromResult(false);

        hospital.Id = Guid.NewGuid();
        hospital.RegistrationDate = DateTime.UtcNow;
        hospital.Status = HospitalStatus.Pending;
        
        _hospitals.Add(hospital);
        return Task.FromResult(true);
    }

    public Task<bool> UpdateHospitalAsync(Hospital hospital)
    {
        var existing = _hospitals.FirstOrDefault(h => h.Id == hospital.Id);
        if (existing == null)
            return Task.FromResult(false);

        var index = _hospitals.IndexOf(existing);
        _hospitals[index] = hospital;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteHospitalAsync(Guid id)
    {
        var hospital = _hospitals.FirstOrDefault(h => h.Id == id);
        if (hospital == null)
            return Task.FromResult(false);

        _hospitals.Remove(hospital);
        return Task.FromResult(true);
    }

    public Task<bool> ApproveHospitalAsync(Guid id)
    {
        var hospital = _hospitals.FirstOrDefault(h => h.Id == id);
        if (hospital == null)
            return Task.FromResult(false);

        hospital.Status = HospitalStatus.Approved;
        return Task.FromResult(true);
    }

    public Task<bool> RejectHospitalAsync(Guid id)
    {
        var hospital = _hospitals.FirstOrDefault(h => h.Id == id);
        if (hospital == null)
            return Task.FromResult(false);

        hospital.Status = HospitalStatus.Rejected;
        return Task.FromResult(true);
    }

    public Task<List<Hospital>> GetHospitalsByStatusAsync(HospitalStatus status)
    {
        var hospitals = _hospitals.Where(h => h.Status == status).ToList();
        return Task.FromResult(hospitals);
    }
}

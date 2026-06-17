using BusinessKit.Application.Availability.Dtos;

namespace BusinessKit.Application.Availability;

public interface IAvailabilityService
{
    Task<AvailabilityResponseDto> GetAvailableSlotsAsync(int staffMemberId, DateTime date, int? businessServiceId = null);
}

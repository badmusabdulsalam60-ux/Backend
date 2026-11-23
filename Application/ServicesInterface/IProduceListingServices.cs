using Application.Dtos;

namespace Application.ServicesInterface
{
    public interface IProduceListingServices
    {
        Task<BaseResponse<CreateProduceListingResponseModel>> CreateAsync(Guid userId, CreateProduceListingRequestModel model);
        Task<BaseResponse<List<ProduceListingDto>>> GetAvailableAsync(string? search);
        Task<BaseResponse<List<ProduceListingDto>>> GetMineAsync(Guid userId);
        Task<BaseResponse<ProduceListingDto>> GetByIdAsync(Guid listingId);
        Task<BaseResponse<ProduceListingDto>> UpdateAsync(Guid userId, Guid listingId, CreateProduceListingRequestModel model);
        Task<BaseResponse<bool>> DeleteAsync(Guid userId, Guid listingId);
    }
}

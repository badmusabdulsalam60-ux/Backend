using Application.Dtos;
using Application.RepositoryInterface;
using Application.ServicesInterface;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.ServicesImplementation
{
    public class ProduceListingServices : IProduceListingServices
    {
        private readonly IProduceListingRepository _listingRepository;
        private readonly IFarmerProfileRepository _farmerRepository;
        private readonly ICropRepository _cropRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProduceListingServices(
            IProduceListingRepository listingRepository,
            IFarmerProfileRepository farmerRepository,
            ICropRepository cropRepository,
            IUnitOfWork unitOfWork)
        {
            _listingRepository = listingRepository;
            _farmerRepository = farmerRepository;
            _cropRepository = cropRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<CreateProduceListingResponseModel>> CreateAsync(
            Guid userId, CreateProduceListingRequestModel model)
        {
            try
            {
                var farmer = await _farmerRepository.GetByUserIdAsync(userId);

                if (farmer == null)
                {
                    return BaseResponse<CreateProduceListingResponseModel>.Failure("Only farmers can create listings.");
                }

                var crop = await _cropRepository.GetByIdAsync(model.CropId);

                if (crop == null || !crop.IsActive)
                {
                    return BaseResponse<CreateProduceListingResponseModel>.Failure("Crop was not found.");
                }

                var listing = new ProduceListing(
                    farmer.Id,
                    crop.Id,
                    model.Title,
                    model.Description,
                    model.PricePerUnit,
                    model.Unit,
                    model.Quantity,
                    model.AvailableFrom);

                await _listingRepository.AddAsync(listing);
                await _unitOfWork.SaveChangesAsync();

                return BaseResponse<CreateProduceListingResponseModel>.Success(
                    "Produce listing created successfully.",
                    new CreateProduceListingResponseModel { Id = listing.Id });
            }
            catch (ArgumentException ex)
            {
                return BaseResponse<CreateProduceListingResponseModel>.Failure(ex.Message);
            }
        }

        public async Task<BaseResponse<List<ProduceListingDto>>> GetAvailableAsync(string? search)
        {
            var listings = await _listingRepository
                .SearchAvailable(search)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var response = listings.Select(MapToDto).ToList();

            return BaseResponse<List<ProduceListingDto>>.Success("Listings retrieved successfully.", response);
        }

        public async Task<BaseResponse<List<ProduceListingDto>>> GetMineAsync(Guid userId)
        {
            var farmer = await _farmerRepository.GetByUserIdAsync(userId);

            if (farmer == null)
            {
                return BaseResponse<List<ProduceListingDto>>.Failure("Farmer profile was not found.");
            }

            var listings = await _listingRepository.Query()
                .Include(x => x.Crop)
                .Include(x => x.FarmerProfile)
                .Where(x => !x.IsDeleted && x.FarmerProfileId == farmer.Id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return BaseResponse<List<ProduceListingDto>>.Success(
                "Your listings were retrieved successfully.",
                listings.Select(MapToDto).ToList());
        }

        public async Task<BaseResponse<ProduceListingDto>> GetByIdAsync(Guid listingId)
        {
            var listing = await _listingRepository.GetByIdWithDetailsAsync(listingId);

            if (listing == null)
            {
                return BaseResponse<ProduceListingDto>.Failure("Listing was not found.");
            }

            return BaseResponse<ProduceListingDto>.Success("Listing retrieved successfully.", MapToDto(listing));
        }
        public async Task<BaseResponse<ProduceListingDto>> UpdateAsync(Guid userId, Guid listingId, CreateProduceListingRequestModel model)
        {
            try
            {
                var farmer = await _farmerRepository.GetByUserIdAsync(userId);
                if (farmer == null) return BaseResponse<ProduceListingDto>.Failure("Only farmers can edit listings.");

                var listing = await _listingRepository.GetByIdWithDetailsAsync(listingId);
                if (listing == null) return BaseResponse<ProduceListingDto>.Failure("Listing was not found.");
                if (listing.FarmerProfileId != farmer.Id) return BaseResponse<ProduceListingDto>.Failure("This listing does not belong to you.");

                listing.UpdateDetails(model.Title, model.Description, model.PricePerUnit, model.Unit, model.Quantity, model.AvailableFrom);
                await _unitOfWork.SaveChangesAsync();
                return BaseResponse<ProduceListingDto>.Success("Listing updated successfully.", MapToDto(listing));
            }
            catch (ArgumentException ex) { return BaseResponse<ProduceListingDto>.Failure(ex.Message); }
            catch (InvalidOperationException ex) { return BaseResponse<ProduceListingDto>.Failure(ex.Message); }
        }

        public async Task<BaseResponse<bool>> DeleteAsync(Guid userId, Guid listingId)
        {
            try
            {
                var farmer = await _farmerRepository.GetByUserIdAsync(userId);
                if (farmer == null) return BaseResponse<bool>.Failure("Only farmers can delete listings.");

                var listing = await _listingRepository.GetByIdWithDetailsAsync(listingId);
                if (listing == null) return BaseResponse<bool>.Failure("Listing was not found.");
                if (listing.FarmerProfileId != farmer.Id) return BaseResponse<bool>.Failure("This listing does not belong to you.");

                listing.Cancel();   // throws if it still has active reservations
                await _unitOfWork.SaveChangesAsync();
                return BaseResponse<bool>.Success("Listing removed.", true);
            }
            catch (InvalidOperationException ex) { return BaseResponse<bool>.Failure(ex.Message); }
        }

        private static ProduceListingDto MapToDto(ProduceListing listing)
        {
            return new ProduceListingDto
            {
                Id = listing.Id,
                FarmerProfileId = listing.FarmerProfileId,
                FarmName = listing.FarmerProfile.FarmName,
                CropId = listing.CropId,
                CropName = listing.Crop.Name,
                Title = listing.Title,
                Description = listing.Description,
                PricePerUnit = listing.PricePerUnit,
                Unit = listing.Unit,
                TotalQuantity = listing.TotalQuantity,
                AvailableQuantity = listing.AvailableQuantity,
                FarmAddress = listing.FarmerProfile.FarmAddress,
                State = listing.FarmerProfile.State,
                AvailableFrom = listing.AvailableFrom,
                Status = listing.Status
            };
        }
    }
}

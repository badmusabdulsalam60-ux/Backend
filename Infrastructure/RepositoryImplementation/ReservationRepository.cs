using AgroConn.Infrastructure.Context;
using Application.RepositoryInterface;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.RepositoryImplementation
{
    public class ReservationRepository(AppDbContext db) : EfGenericRepository<Reservation>(db), IReservationRepository
    {
        public Task<Reservation?> GetByIdWithDetailsAsync(Guid id) =>
      Set.Include(x => x.ProduceListing).ThenInclude(x => x.FarmerProfile)
         .Include(x => x.ProduceListing).ThenInclude(x => x.Crop)
         .Include(x => x.BuyerProfile).ThenInclude(x => x.User)
         .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        public IQueryable<Reservation> ForFarmer(Guid id) =>
            Set.AsNoTracking()
               .Include(x => x.ProduceListing).ThenInclude(x => x.FarmerProfile)
               .Include(x => x.ProduceListing).ThenInclude(x => x.Crop)
               .Include(x => x.BuyerProfile).ThenInclude(x => x.User)
               .Where(x => !x.IsDeleted && x.ProduceListing.FarmerProfileId == id);
        public IQueryable<Reservation> ForBuyer(Guid id) =>
      Set.AsNoTracking()
         .Include(x => x.ProduceListing).ThenInclude(x => x.FarmerProfile)
         .Include(x => x.ProduceListing).ThenInclude(x => x.Crop)
         .Include(x => x.BuyerProfile).ThenInclude(x => x.User)
         .Where(x => !x.IsDeleted && x.BuyerProfileId == id);
        public Task<bool> HasOrderAsync(Guid id) => Db.Orders.AnyAsync(x => x.ReservationId == id && !x.IsDeleted);
    }
}

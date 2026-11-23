using AgroConn.Infrastructure.Context;
using Application.RepositoryInterface;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.RepositoryImplementation
{
    public class OrderRepository(AppDbContext db) : EfGenericRepository<Order>(db), IOrderRepository
    {
        private IQueryable<Order> WithDetails(IQueryable<Order> q) => q
     .Include(x => x.Reservation).ThenInclude(r => r.ProduceListing).ThenInclude(l => l.Crop)
     .Include(x => x.Reservation).ThenInclude(r => r.ProduceListing).ThenInclude(l => l.FarmerProfile)
     .Include(x => x.Reservation).ThenInclude(r => r.BuyerProfile).ThenInclude(b => b.User);

        public Task<Order?> GetByIdWithDetailsAsync(Guid id) =>
            WithDetails(Set).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        public IQueryable<Order> ForFarmer(Guid id) =>
            WithDetails(Set.AsNoTracking())
                .Where(x => !x.IsDeleted && x.Reservation.ProduceListing.FarmerProfileId == id);

        public IQueryable<Order> ForBuyer(Guid id) =>
            WithDetails(Set.AsNoTracking())
                .Where(x => !x.IsDeleted && x.Reservation.BuyerProfileId == id);
    }
}

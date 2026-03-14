using FurnitureStoreData.Context;
using FurnitureStoreData.Models;

namespace FurnitureStoreData.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        public IRepository<Category> Category { get; private set; }
        public IRepository<Product> Product { get; private set; }
        public IRepository<Order> Order { get; private set; }
        public IRepository<OrderDetail> OrderDetail { get; private set; }
        public IRepository<AppUser> AppUser { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new Repository<Category>(_db);
            Product = new Repository<Product>(_db);
            Order = new Repository<Order>(_db);
            OrderDetail = new Repository<OrderDetail>(_db);
            AppUser = new Repository<AppUser>(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
        
        public void Dispose()
        {
            _db.Dispose();
        }
    }
}

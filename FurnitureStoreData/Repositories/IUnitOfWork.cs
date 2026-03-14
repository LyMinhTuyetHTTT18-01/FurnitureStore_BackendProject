using FurnitureStoreData.Models;

namespace FurnitureStoreData.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Category> Category { get; }
        IRepository<Product> Product { get; }
        IRepository<Order> Order { get; }
        IRepository<OrderDetail> OrderDetail { get; }
        IRepository<AppUser> AppUser { get; }
        IRepository<ProductReview> ProductReview { get; }
        IRepository<Wishlist> Wishlist { get; }

        void Save();
        Task SaveAsync();
    }
}

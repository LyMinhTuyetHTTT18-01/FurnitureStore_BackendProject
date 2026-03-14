using System.Linq.Expressions;

namespace FurnitureStoreData.Repositories
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);
        void Update(T entity); // Some paradigms put update outside generic, but it's simpler here.
    }
}

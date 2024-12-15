using System.Linq.Expressions;

namespace SecMan.Interfaces.DAL
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();

        Task<IEnumerable<T>> GetAll(params Expression<Func<T, object>>[] includes);

        Task<IEnumerable<T>> GetAll(params Expression<Func<T, bool>>[] conditions);

        Task<T> GetById(object id);

        Task<T> GetById(object id, params Expression<Func<T, object>>[] includes);

        T Add(T entity);

        void Update(T entity);

        Task<bool> Delete(object id);
    }
}

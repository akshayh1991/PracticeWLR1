using Microsoft.EntityFrameworkCore;
using SecMan.Data.SQLCipher;
using SecMan.Interfaces.DAL;
using System.Linq;
using System.Linq.Expressions;

namespace SecMan.Data.DAL
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly Db _context;
        private readonly DbSet<T> _dbSet;

        // Constructor to inject the DbContext
        public GenericRepository(Db context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAll()
        {
            return await _dbSet.ToListAsync();

        }

        public virtual async Task<IEnumerable<T>> GetAll(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            // Include foreign key entities passed as parameters
            foreach (Expression<Func<T, object>> include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAll(params Expression<Func<T, bool>>[] conditions)
        {
            IQueryable<T> query = _dbSet;

            // Include foreign key entities passed as parameters
            foreach (Expression<Func<T, bool>> condition in conditions)
            {
                query = query.Where(condition);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<T> GetById(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T> GetById(object id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            // Include related entities passed as parameters
            foreach (Expression<Func<T, object>> include in includes)
            {
                query = query.Include(include);
            }

            // Find the entity by id
            return await query.FirstOrDefaultAsync(entity => EF.Property<object>(entity, "Id").Equals(id));
        }

        public virtual T Add(T entity)
        {
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T> result = _dbSet.Add(entity);

            return result.Entity;
        }

        public virtual void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual async Task<bool> Delete(object id)
        {
            T entity = _dbSet.Find(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }
    }
}

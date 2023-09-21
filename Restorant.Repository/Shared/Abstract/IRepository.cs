using Restorant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Restorant.Repository.Shared.Abstract
{
    public interface IRepository<T> where T : BaseModel
    {
        IQueryable<T> GetAll();
        IQueryable<T> GetAll(Expression<Func<T, bool>> predicate);

        T GetById(int Id);
        T GetFirstOrDefault(Expression<Func<T, bool>> predicate);

        public IQueryable<T> GetAllDeleted(Expression<Func<T, bool>> predicate);
        public IQueryable<T> GetAllDeleted();

        void Add(T entity);
        void Update(T entity);
        void Remove(int Id);
        void RemoveRange(IEnumerable<T> entities);
        void AddRange(IEnumerable<T> entities);
        void UpdateRange(IEnumerable<T> entities);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Universal
{
    public interface IRepository<T> where T : IEntity
    {
        Task CreateAsync(T item);
        Task<IReadOnlyCollection<T>> GetAllAsync();
        Task<T> GetAsync(int id);
        Task DeleteAsync(int id);
        Task UpdateAsync(T item);
    }
}
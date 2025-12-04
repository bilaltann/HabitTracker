using HabitTracker.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Interfaces
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        // Tümünü getir
        Task<IEnumerable<TEntity>> GetAllAsync();


        // Tekil getirme
        Task<TEntity> GetByIdAsync(int id);

        // Ekleme (Geriye eklenen nesneyi döner)
        Task<TEntity> CreateAsync (TEntity entity);

        // Güncelleme
        Task UpdateAsync(TEntity entity);

        // Silme
        Task DeleteAsync(int id); // Veya direk entity alabilir
    }
}

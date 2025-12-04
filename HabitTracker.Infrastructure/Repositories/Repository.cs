using HabitTracker.Application.Interfaces;
using HabitTracker.Domain.Common;
using HabitTracker.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity:BaseEntity
    {
        private readonly HabitTrackerDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public Repository(HabitTrackerDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>(); // örn: user geldiyse Users tablosuna odaklan
        }
      

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync(); // Transaction (işlem) onayı
            return entity; // ID'si oluşmuş hali döner
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity); // Hard Delete (Veritabanından tamamen siler)
                // Soft Delete (IsDeleted = true) yapmak istersen burayı güncellemelisin.
                await _context.SaveChangesAsync();
            }
        }


        public async Task UpdateAsync(TEntity entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        
    }
}

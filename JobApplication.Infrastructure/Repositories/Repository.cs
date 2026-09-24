using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using JobApplication.Infrastructure.Persistence.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobApplication.Infrastructure.Repositories;

public class Repository<T>(ApplicationDbContext dbContext) : IRepository<T> where T : BaseEntity
{

    private readonly DbSet<T> _dbSet = dbContext.Set<T>();

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ck = default)
        => await _dbSet.ToListAsync(ck);

    public async Task<T?> GetByIdAsync(int id, CancellationToken ck = default)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(entity => entity.Id == id);

    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);

    public void Update(T entity)
        => _dbSet.Update(entity);
    public void Delete(T entity)
        => _dbSet.Remove(entity);

    public IQueryable<T> GetQueryable() => _dbSet.AsQueryable();
}

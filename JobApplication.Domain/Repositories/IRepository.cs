using JobApplication.Domain.Entities.Business;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ck = default);
    Task<T?> GetByIdAsync(int id, CancellationToken ck = default);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

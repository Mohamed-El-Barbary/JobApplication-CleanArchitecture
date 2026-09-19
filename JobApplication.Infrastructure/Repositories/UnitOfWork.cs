using JobApplication.Domain.Entities.Business;
using JobApplication.Domain.Repositories;
using JobApplication.Infrastructure.Persistence.Data.DbContexts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    private readonly ConcurrentDictionary<Type, object> _repos = new();
    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);

        if (_repos.TryGetValue(type, out var repo))
            return (IRepository<T>)repo;

        var newRepo = new Repository<T>(dbContext);

        _repos.TryAdd(type, newRepo);

        return newRepo;
    }

    public Task<int> SaveChangesAsync(CancellationToken ck = default)
            => dbContext.SaveChangesAsync(ck);
}

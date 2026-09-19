using JobApplication.Domain.Entities.Business;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Repositories;

public interface IUnitOfWork
{

    IRepository<T> Repository<T>() where T : BaseEntity;

    Task<int> SaveChangesAsync(CancellationToken ck = default);

}

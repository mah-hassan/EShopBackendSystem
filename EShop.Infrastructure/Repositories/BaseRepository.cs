using EShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using EShop.Domain.Abstractions;
using EShop.Domain.Abstractions.Specifications;
using EShop.Infrastructure.Specifications;
namespace EShop.Infrastructure.Repositories;

internal abstract class BaseRepository<TEntity>

    : IBaseRepository<TEntity>
    where TEntity : Entity
{
    protected readonly ApplicationDbContext dbContext;

    protected BaseRepository(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public void Add(TEntity entity)
        => dbContext.Set<TEntity>().Add(entity);

    public void Delete(TEntity entity)
        => dbContext.Set<TEntity>().Remove(entity);

    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await dbContext.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<TEntity?> GetByIdAsync(Guid id) 
        => await dbContext.Set<TEntity>().FindAsync(id);

    public void Update(TEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        dbContext.Set<TEntity>().Update(entity);
    }
    protected IQueryable<TEntity> ApplaySpecifications(Specification<TEntity> specification)
    {
        return SpecificationEvaluater
             .Evaluate(dbContext.Set<TEntity>(), specification);
    }
}

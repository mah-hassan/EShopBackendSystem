using EShop.Application.Abstractions;
using EShop.Infrastructure.Data;

namespace EShop.Infrastructure.Services;

internal sealed class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => await dbContext.SaveChangesAsync(cancellationToken);
}

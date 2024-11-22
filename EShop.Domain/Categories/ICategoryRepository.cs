using EShop.Domain.Abstractions;
using EShop.Domain.Entities;
namespace EShop.Domain.Categories;

public interface ICategoryRepository
    : IBaseRepository<Category>
{
    Task<bool> IsNameExsists(string name);
}

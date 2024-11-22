using Riok.Mapperly.Abstractions;

namespace EShop.Application.Abstractions.Mappers;

public partial class Mapper 
{ 
    public partial Category MapToCategory(AddCategoryRequest dto);

    public partial CategoryResponse MapToCategoryResponse(Category category);

    public partial CategoryDetails MapToCategoryDetails(Category category);

}
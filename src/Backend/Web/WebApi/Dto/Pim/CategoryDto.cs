using System.Collections.Generic;
using System.Linq;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class CategoryDto
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Name { get; set; }

        public IEnumerable<CategoryDto> SubCategoriesDtos { get; set; }

        public CategoryDto()
        {
            SubCategoriesDtos = Enumerable.Empty<CategoryDto>();
        }

        public CategoryDto(Category category, bool withSubcategories = false)
        {
            Id = category.Id;
            ParentId = category.ParentId;
            Name = category.Name;

            SubCategoriesDtos = withSubcategories
                ? category.SubCategories.Where(sc => sc.DeleteTime == null).Select(sc => new CategoryDto(sc, true)).ToList()
                : Enumerable.Empty<CategoryDto>();
        }

        public Category ToEntity()
        {
            return new Category()
            {
                Id = Id,
                ParentId = ParentId,
                Name = Name,
                SubCategories = SubCategoriesDtos.Select(sc => sc.ToEntity()).ToList()
            };
        }
    }
}
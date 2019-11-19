using System;
using System.Collections.Generic;
using System.Linq;
using Company.Common.Models.Pim;
using Attribute = Company.Common.Models.Pim.Attribute;

namespace Web1cApi.Dto
{
    public class AttributeDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? GroupId { get; set; }

        /* List Type */
        public int? ListId { get; set; }

        /* Str or Text Type */
        public string Template { get; set; }

        public int? MaxLength { get; set; }

        public int? MinLength { get; set; }

        /* Num Type */
        public int? Max { get; set; }

        public int? Min { get; set; }

        /* Date Type */
        public DateTime? MinDate { get; set; }

        public DateTime? MaxDate { get; set; }

        public List<int> CategoriesIds { get; set; } = new List<int>();

        public List<AttributePermissionDto> Permissions { get; set; } = new List<AttributePermissionDto>();

        public AttributeType Type { get; set; }

        public AttributeDto()
        {
        }

        public Attribute ToEntity()
        {
            return new Attribute
            {
                Id = Id,
                Name = Name,
                Type = Type,
                GroupId = GroupId,
                ListId = ListId,
                Template = Template,
                MaxLength = MaxLength,
                MinLength = MinLength,
                Max = Max,
                Min = Min,
                MinDate = MinDate,
                MaxDate = MaxDate,
                AttributePermissions = Permissions.Select(p => p.ToEntity()).ToList(),
                AttributeCategories = CategoriesIds.Select(id => new AttributeCategory() { CategoryId = id }).ToList()
            };
        }

        public AttributeDto(Attribute attribute, bool withCategories = false)
        {
            Id = attribute.Id;
            Name = attribute.Name;
            Type = attribute.Type;
            GroupId = attribute.GroupId;
            ListId = attribute.ListId;
            Template = attribute.Template;
            MaxLength = attribute.MaxLength;
            MinLength = attribute.MinLength;
            Max = attribute.Max;
            Min = attribute.Min;
            MinDate = attribute.MinDate;
            MaxDate = attribute.MaxDate;

            Permissions = attribute.AttributePermissions.Select(ap => new AttributePermissionDto(ap)).ToList();

            CategoriesIds = withCategories
                ? attribute.AttributeCategories.Select(ac => ac.CategoryId).ToList()
                : new List<int>();
        }
    }
}
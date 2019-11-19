using System;
using System.Collections.Generic;

namespace Company.Common.Models.Pim
{
    public class Attribute
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int? CreatorId { get; set; }

        public int? DeleterId { get; set; }

        public AttributeType Type { get; set; }

        /* Str or Text Type */
        public string Template { get; set; }

        public int? MaxLength { get; set; }

        public int? MinLength { get; set; }

        /* Num Type */
        public int? Max { get; set; }

        public int? Min { get; set; }

        /* List Type */
        public int? ListId { get; set; }

        public List List { get; set; }

        /* */
        public int? GroupId { get; set; }

        /* Date Type */
        public DateTime? MinDate { get; set; }

        public DateTime? MaxDate { get; set; }

        public AttributeGroup AttributeGroup { get; set; }

        public ICollection<AttributeCategory> AttributeCategories { get; set; } = new List<AttributeCategory>();

        public ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();

        public ICollection<AttributePermission> AttributePermissions { get; set; } = new List<AttributePermission>();

        public Attribute()
        {

        }
    }

    public enum AttributeType
    {
        String = 1,
        Number = 2,
        Boolean = 3,
        List = 4,
        Text = 5,
        Date = 6
    }
}

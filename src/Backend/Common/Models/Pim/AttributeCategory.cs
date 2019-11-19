using Company.Common.Enums;

namespace Company.Common.Models.Pim
{
    public class AttributeCategory
    {
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public int AttributeId { get; set; }

        public ModelLevel ModelLevel { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>
        /// Is filtered in search of products in B2B app
        /// </summary>
        public bool IsFiltered { get; set; }

        /// <summary>
        /// Is visible in product card in B2B app
        /// </summary>
        public bool IsVisibleInProductCard { get; set; }

        /// <summary>
        /// Key attribute (for analytics)
        /// </summary>
        public bool IsKey { get; set; }

        public virtual Attribute Attribute { get; set; }
    }
}
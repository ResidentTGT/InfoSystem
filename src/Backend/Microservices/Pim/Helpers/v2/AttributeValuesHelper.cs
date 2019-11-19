using System.Collections.Generic;
using Company.Common.Models.Pim;

namespace Company.Pim.Helpers.v2
{
    public class AttributeValuesHelper
    {
        public static string CreateSearchString(AttributeValue attributeValue, string listValue)
        {
            return (attributeValue.StrValue + " " + attributeValue.NumValue?.ToString() + " " + attributeValue.DateValue?.ToString() + " " + (attributeValue.BoolValue != null ? ((bool)attributeValue.BoolValue ? "Да" : "Нет") : null) + " " + listValue).Trim();
        }
    }

    public class Search
    {
        public Search()
        {

        }

        public List<string> sku { get; set; } = new List<string>();
        public List<string> names { get; set; } = new List<string>();
        public List<int?> imports { get; set; } = new List<int?>();
        public List<AttrParams> unnameds { get; set; } = new List<AttrParams>();
        public List<AttrParams> attrParams { get; set; } = new List<AttrParams>();
    }

    public class AttrParams
    {
        public string name { get; set; } = "";
        public List<string> values { get; set; } = new List<string>();
    }
}

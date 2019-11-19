using System.Collections.Generic;
using System.Linq;

namespace Company.Common.Models.Pim
{
    public sealed class PimResourcePermissions
    {
        public const string Title = "Наименование";
        public const string Attributes = "Атрибуты";
        public const string Categories = "Категории";
        public const string Docs = "Документы";
        public const string Media = "Медиа";
        public const string Product = "Товар";

    public static IEnumerable<string> GetPermissionsNames()
        {
            return typeof(PimResourcePermissions)
                .GetFields()
                .Where(f => f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue())
                .ToList();
        }
    }
}

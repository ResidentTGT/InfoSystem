using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Pim.Configs
{
    public static class ExportExcelConfig
    {
        public static Dictionary<char, string> Headers = new Dictionary<char, string>()
        {
            {'A', "Артикул компании (SKU)"},
            {'B', "SyncId"},
            {'C', "ParentId"},
            {'D', "Наименование изделия"},
            {'E', "Сцепка (группа товара$наименование изделия)"},
            {'F', "Фото Front"},
            {'G', "Фото Back"},
            {'H', "Фото Side"},
            {'I', "Фото 360"},
            {'J', "Фото Above"},
            {'K', "Фото Detail(crop)"},
            {'L', "Фото Detail(other)"},
            {'M', "Video"},
            {'N', ""},
            {'O', ""},
            {'P', ""},
            {'Q', ""},
            {'R', ""},
        };
    }
}
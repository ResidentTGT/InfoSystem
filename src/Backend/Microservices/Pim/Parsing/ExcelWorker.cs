using OfficeOpenXml;
using System;
using System.Text.RegularExpressions;

namespace Company.Pim.Parsing
{
    internal class ExcelWorker
    {
        internal string GetColumnNameFromCell(ExcelRangeBase cell) => new Regex(@"[\d\.]").Replace(cell.Address, "");

        internal int GetColumnIndexFromCell(ExcelRangeBase cell) => ConvertColumnLetterToColumnIndex(new Regex(@"[\d\.]").Replace(cell.Address, ""));

        internal int GetRowIndexFromCell(ExcelRangeBase cell) => Convert.ToInt32(new Regex(@"[^\d]").Replace(cell.Address, ""));

        internal int ConvertColumnLetterToColumnIndex(string columnLetter)
        {
            columnLetter = columnLetter.ToUpper();
            int sum = 0;

            for (int i = 0; i < columnLetter.Length; i++)
            {
                sum *= 26;
                sum += (columnLetter[i] - 'A' + 1);
            }
            return sum;
        }
    }
}

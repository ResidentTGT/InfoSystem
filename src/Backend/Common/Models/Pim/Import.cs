using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Company.Common.Models.Pim
{
    public class Import
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int CreatorId { get; set; }

        public int? DeleterId { get; set; }

        public int ModelCount { get; set; }

        public int ColorModelCount { get; set; }

        public int RangeSizeModelCount { get; set; }

        public int ModelSuccessCount { get; set; }

        public int ColorModelSuccessCount { get; set; }

        public int RangeSizeModelSuccessCount { get; set; }

        public int ErrorCount { get; set; }

        public int FileId { get; set; }

        public bool? FinishedSuccess { get; set; }

        public string Error { get; set; }

        public virtual ICollection<Product> Products { get; set; }

        [NotMapped]
        public int TotalCount => ModelCount + ColorModelCount + RangeSizeModelCount;
        [NotMapped]
        public int SuccessCount => ModelSuccessCount + ColorModelSuccessCount + RangeSizeModelSuccessCount;

        public Import()
        {
        }
    }
}
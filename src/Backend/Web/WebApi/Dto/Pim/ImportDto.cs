using System;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class ImportDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

        public int ImporterId { get; set; }

        public string ImporterName { get; set; }

        public int TotalCount { get; set; }

        public int SuccessCount { get; set; }

        public int ErrorCount { get; set; }

        public int ModelCount { get; set; }

        public int ColorModelCount { get; set; }

        public int RangeSizeModelCount { get; set; }

        public int ModelSuccessCount { get; set; }

        public int ColorModelSuccessCount { get; set; }

        public int RangeSizeModelSuccessCount { get; set; }

        public int FileId { get; set; }

        public bool? FinishedSuccess { get; set; }

        public string Error { get; set; }

        public ImportDto()
        {

        }

        public ImportDto(Import import, string importerName)
        {
            Id = import.Id;
            Name = import.Name;
            CreateDate = import.CreateTime;
            ImporterId = import.CreatorId;
            ImporterName = importerName;
            ModelCount = import.ModelCount;
            ColorModelCount = import.ColorModelCount;
            RangeSizeModelCount = import.RangeSizeModelCount;
            ModelSuccessCount = import.ModelSuccessCount;
            ColorModelSuccessCount = import.ColorModelSuccessCount;
            RangeSizeModelSuccessCount = import.RangeSizeModelSuccessCount;
            TotalCount = import.TotalCount;
            SuccessCount = import.SuccessCount;
            ErrorCount = import.ErrorCount;
            FileId = import.FileId;
            FinishedSuccess = import.FinishedSuccess;

        }
    }
}

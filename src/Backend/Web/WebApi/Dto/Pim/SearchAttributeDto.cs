namespace WebApi.Dto.Pim
{
    public class SearchAttributeDto
    {
        public string AttributeName { get; set; }

        public string StrValue { get; set; }

        public double? NumValue { get; set; }

        public bool? BoolValue { get; set; }
    }
}

using System;

namespace WebApi.Dto.Pim
{
    public class FileDto
    {
        public int Id { get; set; }

        public string Path { get; set; }

        public string Name { get; set; }

        public int UserId { get; set; }

        public DateTime CreationTime { get; set; }
    }
}

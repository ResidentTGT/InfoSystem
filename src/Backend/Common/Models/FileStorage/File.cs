using System;

namespace Company.Common.Models.FileStorage
{
    public class File
    {
        public int Id { get; set; }

        public string Path { get; set; }

        public string Name { get; set; }

        public int UserId { get; set; }

        public DateTime CreationTime { get; set; }
    }
}

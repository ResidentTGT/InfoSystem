using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Npgsql;
using Company.Common.Models.FileStorage;


namespace Company.FileStorage
{
    public class FileStorageManager
    {
        const int _numberOfPaddedSymbols = 12;
        public string BasePath { get; private set; }

        public FileStorageManager(string basePath)
        {
            BasePath = basePath;
        }

        public Common.Models.FileStorage.File SaveFile(FileStorageContext ctx, IFormFile fileStream, int userId)
        {
            var lastFile = ctx.Files.LastOrDefault();

            int lastDbId = lastFile == null ? 0 : lastFile.Id;
        
            var file = new Common.Models.FileStorage.File()
            {
                UserId = userId,
                Name = fileStream.FileName,
                Path = GenerateServerFilePathFromId(lastDbId),
                CreationTime = DateTime.Now
            };

            var fullDirectoryPath = Path.Combine(BasePath, Path.GetDirectoryName(file.Path));

            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);  
            }

            SaveToFileSystem(fileStream, Path.Combine(BasePath, file.Path));

            ctx.Files.Add(file);
            ctx.SaveChanges();

            return file;
        }

        public Common.Models.FileStorage.File Get(FileStorageContext ctx, int id)
        {
            return ctx.Files.Find(id);
        }

        public Common.Models.FileStorage.File Remove(FileStorageContext ctx, int id)
        {
            var file = ctx.Files.FirstOrDefault(f => f.Id == id);

            if (file != null)
            {
                ctx.Files.Remove(file);

                ctx.SaveChanges();

                System.IO.File.Delete(Path.Combine(BasePath, file.Path));

                return file;
            }
            else
            {
                throw new FileNotFoundException("No such file.");
            } 
        }

        private void SaveToFileSystem(IFormFile fileStream, string serverFilePath)
        {
            //if (fileStream.CanSeek)
             //   fileStream.Seek(0, SeekOrigin.Begin);

            using (var file = System.IO.File.Create(serverFilePath))
            {
                try
                {
                    fileStream.CopyTo(file);
                }
                catch
                {
                    TryDeleteFile(serverFilePath);
                    throw;
                }
            }
        }

        private string GenerateServerFilePathFromId(int id)
        {
            string paddedId = (id + 1).ToString($"D{_numberOfPaddedSymbols}");

            // This Regex made from the 00001234 to the 00\00\12\34
            return Path.Combine(Regex.Replace(paddedId.Remove(paddedId.Length - 2), ".{2}", "$0/"), DateTime.Now.Ticks.ToString());
        }

        private bool TryDeleteFile(string path)
        {
            try
            {
                System.IO.File.Delete(path);
                return true;
            }
            catch(Exception ex)
            {
                // TODO log!
                return false;
            }
        }
    }
}

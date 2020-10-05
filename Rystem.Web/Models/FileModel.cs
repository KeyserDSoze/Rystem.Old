using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.Web.Models
{
    public class FileModel
    {
        public string FullName { get; }
        public string Name => FullName.Split('/').Last();
        public bool IsDirectory => string.IsNullOrWhiteSpace(Name);
        public string ContentType { get; }
        public DateTime ModificationTime { get; }
        public string RandomId { get; } = Guid.NewGuid().ToString("N");
        public FileModel(string fullName, DateTime modificationTime)
        {
            this.FullName = fullName;
            this.ContentType = fullName.Split('.').Last();
            this.ModificationTime = modificationTime;
        }
        public FileModel(string contentType, string fullName, DateTime modificationTime)
        {
            this.FullName = fullName;
            this.ContentType = contentType;
            this.ModificationTime = modificationTime;
        }
        public IEnumerable<string> GetPath(int skip = 0)
        {
            string[] value = this.FullName.Split('/');
            return value.Skip(skip).Take(value.Length - skip -  1);
        }
        public string GetRightIcon()
            => FullName.Split('.').Last().ToLower() switch
            {
                "pdf" => "fa-file-pdf",
                "docx" => "fa-file-word",
                "doc" => "fa-file-word",
                "xlsx" => "fa-file-excel",
                "xls" => "fa-file-excel",
                _ => "fa-file-download"
            };
        public bool IsImage()
            => FullName.Split('.').Last().ToLower() switch
            {
                "apng" => true,
                "bmp" => true,
                "gif" => true,
                "ico" => true,
                "cur" => true,
                "jpg" => true,
                "jpeg" => true,
                "jfif" => true,
                "pjpeg" => true,
                "pjp" => true,
                "png" => true,
                "svg" => true,
                "tif" => true,
                "tiff" => true,
                "webp" => true,
                _ => false
            };
    }
    public class DirectoryModel
    {
        public string Id { get; } = $"folder-{Guid.NewGuid():N}";
        public List<FileModel> Files { get; }
        public Dictionary<string, DirectoryModel> Directories { get; } = new Dictionary<string, DirectoryModel>();
        public string Name { get; }
        public string LocalizedName { get; }
        public DirectoryModel Father { get; }
        public bool IsRoot => Father == null;
        public DirectoryModel(List<FileModel> files, string name, string localizedName, DirectoryModel father)
        {
            this.Files = files;
            this.Name = name;
            this.Father = father;
            this.LocalizedName = localizedName;
        }
        public static DirectoryModel FromFiles(IEnumerable<FileModel> files, IStringLocalizer stringLocalizer, int skip = 0)
        {
            DirectoryModel root = new DirectoryModel(new List<FileModel>(), "root", "root", null);
            foreach (FileModel file in files)
            {
                DirectoryModel next = root;
                foreach (var directory in file.GetPath(skip))
                {
                    if (string.IsNullOrWhiteSpace(directory))
                        break;
                    if (!next.Directories.ContainsKey(directory))
                        next.Directories.Add(directory, new DirectoryModel(new List<FileModel>(), directory, stringLocalizer == null ? directory : stringLocalizer[directory], next));
                    next = next.Directories[directory];
                }
                if (!file.IsDirectory)
                    next.Files.Add(file);
            }
            return root;
        }
    }
}
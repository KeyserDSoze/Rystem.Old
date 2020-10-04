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
        public string ContentType { get; }
        public DateTime ModificationTime { get; }
        public FileModel(string contentType, string fullName, DateTime modificationTime)
        {
            this.FullName = fullName;
            this.ContentType = contentType;
            this.ModificationTime = modificationTime;
        }
        public IEnumerable<string> GetPath()
        {
            string[] value = this.FullName.Split('/');
            return value.Take(value.Length - 1);
        }
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
        public static DirectoryModel FromFiles(IEnumerable<FileModel> files, IStringLocalizer stringLocalizer)
        {
            DirectoryModel root = new DirectoryModel(new List<FileModel>(), "root", "root", null);
            foreach (FileModel file in files)
            {
                DirectoryModel next = root;
                foreach (var directory in file.GetPath())
                {
                    if (!next.Directories.ContainsKey(directory))
                        next.Directories.Add(directory, new DirectoryModel(new List<FileModel>(), directory, stringLocalizer == null ? directory : stringLocalizer[directory], next));
                    next = next.Directories[directory];
                }
                next.Files.Add(file);
            }
            return root;
        }
    }
}
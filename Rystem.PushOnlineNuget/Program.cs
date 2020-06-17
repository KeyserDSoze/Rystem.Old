using System.Management;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using System.Text;
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Linq;
using System.Drawing;

namespace Rystem.PushOnlineNuget
{
    public class Program
    {
        static void Main(string[] args)
        {
            foreach(string value in RunScript())
            {
                if (value.ToLower().Contains("your package was pushed."))
                    Console.ForegroundColor = ConsoleColor.Green;
                else
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(value);
                Console.ResetColor();
            }
        }

        private const string BasicCommand = "nuget push {0} -source {1}";
        private static List<string> Repositories = new List<string>() {
            "\"TelcoNugetDevops\" -ApiKey az",
            "\"E:\\nugetrepository\""
        };
        private class Nuget
        {
            public string CompleteName { get; }
            public string Name { get; }
            public string Uri { get; }
            public int Major { get; } = -1;
            public int Minor { get; } = -1;
            public int Patch { get; } = -1;
            public Nuget(FileInfo fileInfo)
            {
                this.Uri = fileInfo.FullName.Replace(fileInfo.Name, string.Empty);
                string[] names = fileInfo.Name.Split('.');
                this.Name = string.Empty;
                this.CompleteName = fileInfo.Name;
                for (int i = names.Length - 2; i >= 0; i--)
                {
                    if (Patch == -1)
                        Patch = int.Parse(names[i]);
                    else if (Minor == -1)
                        Minor = int.Parse(names[i]);
                    else if (Major == -1)
                        Major = int.Parse(names[i]);
                    else
                        this.Name += $"{names[i]}.";
                }
            }
        }
        private static List<Nuget> GetNugets(DirectoryInfo directoryInfo)
        {
            List<Nuget> nugets = new List<Nuget>();
            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                if (fileInfo.Name.EndsWith(".nupkg") && !fileInfo.Name.Contains(".symbols."))
                    nugets.Add(new Nuget(fileInfo));
            foreach (DirectoryInfo directory in directoryInfo.GetDirectories().Where(x => !x.Name.ToLower().Contains("debug")))
                nugets.AddRange(GetNugets(directory));
            return nugets;
        }
        private static IEnumerable<string> RunScript()
        {
            List<Nuget> nugets = GetNugets(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent);
            Collection<PSObject> results = new Collection<PSObject>();
            foreach (var nugetsByName in nugets.GroupBy(x => x.Name))
            {
                Nuget first = nugetsByName.OrderByDescending(x => x.Major).ThenByDescending(x => x.Minor).ThenByDescending(x => x.Patch).FirstOrDefault();
                foreach (string repository in Repositories)
                {
                    using (Runspace runspace = RunspaceFactory.CreateRunspace())
                    {
                        runspace.Open();
                        Pipeline pipeline = runspace.CreatePipeline();
                        pipeline.Commands.AddScript("Set-ExecutionPolicy -Scope Process -ExecutionPolicy Unrestricted");
                        pipeline.Commands.AddScript($"cd {first.Uri}");
                        pipeline.Commands.AddScript(string.Format(BasicCommand, first.CompleteName, repository));
                        pipeline.Commands.Add("Out-String");
                        foreach (var t in pipeline.Invoke())
                            results.Add(t);
                    }
                }
            }
            foreach (PSObject obj in results)
                yield return obj.ToString();
        }
    }
}
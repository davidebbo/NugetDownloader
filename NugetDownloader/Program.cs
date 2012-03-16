using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuGet;
using System.IO;

namespace NugetDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Syntax: NugetDownloader targetFolder numOfPackages");
                return;
            }

            var downloadFolder = new DirectoryInfo(args[0]);
            var count = Int32.Parse(args[1]);

            var remoteRepo = new DataServicePackageRepository(
                new Uri("https://nuget.org/api/v2/"));

            var localRepo = new LocalPackageRepository(downloadFolder.FullName);

            var packages = remoteRepo.GetPackages()
                .Where(p => p.IsLatestVersion)
                .OrderByDescending(p => p.DownloadCount)
                .Take(count);

            foreach (IPackage package in packages)
            {
                Console.WriteLine(package.Id + " " + package.Version);

                localRepo.AddPackage(package);
            }

            // The packages end up in a folder, so move them directly in the top folder
            foreach (var file in downloadFolder.EnumerateFiles("*.nupkg", SearchOption.AllDirectories))
            {
                file.MoveTo(Path.Combine(downloadFolder.FullName, file.Name));
            }

            // Delete the folders
            foreach (var dir in downloadFolder.EnumerateDirectories())
            {
                dir.Delete();
            }
        }
    }
}

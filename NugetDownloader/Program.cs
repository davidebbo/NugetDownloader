using NuGet;
using System;
using System.IO;
using System.Linq;

namespace NugetDownloader
{
    class Program
    {
        static readonly Uri nugetUri = new Uri("https://nuget.org/api/v2/");

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Syntax: NugetDownloader targetFolder numOfPackages");
                return;
            }

            var downloadFolder = new DirectoryInfo(args[0]);
            var packageCount = Int32.Parse(args[1]);
            var localRepository = new LocalPackageRepository(downloadFolder.FullName);

            DataServicePackageRepository remoteRepo = null;
            try
            {
                remoteRepo = new DataServicePackageRepository(nugetUri);

                var packages = remoteRepo.GetPackages()
                    .Where(p => p.IsLatestVersion)
                    .OrderByDescending(p => p.DownloadCount)
                    .Take(packageCount);

                foreach (IPackage eachPackage in packages)
                {
                    try
                    {
                        Console.Write(eachPackage.Id + " " + eachPackage.Version + " ... ");
                        localRepository.AddPackage(eachPackage);
                        Console.WriteLine("OK");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("FAILED");
                        Console.WriteLine("[EXCEPTION] " + ex.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("[EXCEPTION] " + ex.Message);
                return;
            }

            foreach (var file in downloadFolder.EnumerateFiles("*.nupkg", SearchOption.AllDirectories))
            {
                try
                {
                    var targetPath = Path.Combine(downloadFolder.FullName, file.Name);
                    if (!File.Exists(targetPath))
                        file.MoveTo(targetPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[EXCEPTION] " + ex.Message);
                }
            }

            foreach (var dir in downloadFolder.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}

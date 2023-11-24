#pragma warning disable CS8600
#pragma warning disable CS8604
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;


namespace EXILED_DLL_Archiver
{
    public class Program
    {
        public static void Main(String[] args)
        {
            string path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

            try
            {
                CreateTarGZ(Path.Combine(path, "filtrage_start_hyster.tar.gz"), path);
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
                Environment.Exit(0);
            }
            
        }

        private static void CreateTarGZ(string tgzFilename, string sourceDirectory)
        {
            Stream outStream = File.Create(tgzFilename);
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

            tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

            AddDirectoryFilesToTar(tarArchive, Path.Combine(sourceDirectory, "filtrage_start_hyster"), true);

            tarArchive.Close();
        }

        private static void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);

            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {

                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    if (!directory.Contains("bin"))
                        AddDirectoryFilesToTar(tarArchive, directory, recurse);
            }
        }
    }
}

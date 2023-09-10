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
            string fileName = string.Empty;
            string destFile = string.Empty;

            string exiled_plugins_path = Path.Combine(path, "EXILED", "Plugins");
            string exiled_plugins_deps_path = Path.Combine(exiled_plugins_path, "dependencies");
            string nw_plugin_path = Path.Combine(path, "SCP Secret Laboratory", "PluginAPI", "plugins", "global");
            string nw_plugin_deps_path = Path.Combine(nw_plugin_path, "dependencies");

            List<string> plugins = new List<string> { "Exiled.CreditTags", "Exiled.CustomItems", "Exiled.CustomRoles", "Exiled.Events", "Exiled.Permissions", "Exiled.Updater" };
            List<string> pluginsDep = new List<string> { "0Harmony", "Exiled.API", "SemanticVersioning", "Mono.Posix", "System.ComponentModel.DataAnnotations" };
            List<string> nwDep = new List<string> { "Exiled.API" };

            try
            {
                Directory.CreateDirectory(exiled_plugins_deps_path);
                Directory.CreateDirectory(nw_plugin_deps_path);

                try
                {
                    foreach (string str in plugins)
                    {
                        fileName = Path.Combine(path, str + ".dll");
                        destFile = Path.Combine(exiled_plugins_path, str + ".dll");
                        File.Copy(fileName, destFile, true);
                    }

                    foreach (string str in pluginsDep)
                    {
                        fileName = Path.Combine(path, str + ".dll");
                        destFile = Path.Combine(exiled_plugins_deps_path, str + ".dll");
                        File.Copy(fileName, destFile, true);
                    }

                    foreach (string str in nwDep)
                    {
                        fileName = Path.Combine(path, str + ".dll");
                        destFile = Path.Combine(nw_plugin_deps_path, str + ".dll");
                        File.Copy(fileName, destFile, true);
                    }

                    File.Copy(Path.Combine(path, "Exiled.Loader.dll"), Path.Combine(nw_plugin_path, "Exiled.Loader.dll"), true);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine("Missing dll: " + e.Message);
                    Console.WriteLine("Mono.Posix and System.ComponentModel.DataAnnotations need to be manually added if missing");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                CreateTarGZ(Path.Combine(path, "Exiled.tar.gz"), path);
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

            AddDirectoryFilesToTar(tarArchive, Path.Combine(sourceDirectory, "EXILED"), true);
            AddDirectoryFilesToTar(tarArchive, Path.Combine(sourceDirectory, "SCP Secret Laboratory"), true);

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
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
            }
        }
    }
}

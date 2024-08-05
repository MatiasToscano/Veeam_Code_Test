using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Veeam_Matias_Toscano
{
    internal class Program
    {
        static void Main(string[] args)
    {           
            if (args.Length < 4)
            {
                Console.WriteLine("These arguments are needed for synchronize: 'Source Path', 'Replica Path', 'Interval' and 'Log File'." +
                    "Please respect the order of the arguments.");
                return;
            }

            string sourcePath = args[0];
            string replicaPath = args[1];
            int interval = int.Parse(args[2]);
            string logFile = args[3];

            while (true)
            {
                try
                {
                    SynchronizeFolders(sourcePath, replicaPath, logFile);
                }
                catch (Exception ex)
                {
                    Log($"Error: {ex.Message}", logFile);
                }

                Thread.Sleep(interval * 1000);
            }
        }

        static void SynchronizeFolders(string sourcePath, string replicaPath, string logFile)
        {
            foreach (var sourceFile in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                string relativePath = sourceFile.Substring(sourcePath.Length + 1);
                string replicaFile = Path.Combine(replicaPath, relativePath);

                if (!File.Exists(replicaFile) || !FilesAreEqual(sourceFile, replicaFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(replicaFile));
                    File.Copy(sourceFile, replicaFile, true);
                    Log($"File copied: {relativePath}", logFile);
                }
            }

            foreach (var replicaFile in Directory.GetFiles(replicaPath, "*", SearchOption.AllDirectories))
            {
                string relativePath = replicaFile.Substring(replicaPath.Length + 1);
                string sourceFile = Path.Combine(sourcePath, relativePath);

                if (!File.Exists(sourceFile))
                {
                    File.Delete(replicaFile);
                    Log($"File deleted: {relativePath}", logFile);
                }
            }
        }

        static bool FilesAreEqual(string file1, string file2)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash1 = md5.ComputeHash(File.ReadAllBytes(file1));
                byte[] hash2 = md5.ComputeHash(File.ReadAllBytes(file2));

                return Encoding.UTF8.GetString(hash1) == Encoding.UTF8.GetString(hash2);
            }
        }

        static void Log(string message, string logFile)
        {
            Console.WriteLine(message);
            File.AppendAllText(logFile, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
    }
}

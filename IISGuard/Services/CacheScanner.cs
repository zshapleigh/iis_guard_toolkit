using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace IISGuard.Services
{
    public static class CacheScanner
    {
        public static void ScanCacheHealth()
        {
            Console.WriteLine("[CacheScanner] Scanning for common IIS cache corruption patterns...");
            ScanForLargeTemporaryFiles();
            ScanForCorruptCache();
        }

        private static void ScanForLargeTemporaryFiles()
        {
            Console.WriteLine("[CacheScanner] Scanning for large temporary files...");
            long largeFileThreshold = 1024 * 1024 * 1024; // 1GB
            string systemRoot = Environment.GetEnvironmentVariable("SystemRoot");

            List<string> tempDirectories = new List<string>
            {
                Path.Combine(systemRoot, "Temp"),
                // Adding .NET Framework temporary files paths - this will need to be dynamic or cover common versions
                Path.Combine(systemRoot, "Microsoft.NET", "Framework64", "v4.0.30319", "Temporary ASP.NET Files"),
                Path.Combine(systemRoot, "Microsoft.NET", "Framework", "v4.0.30319", "Temporary ASP.NET Files"),
                Path.Combine(systemRoot, "Microsoft.NET", "Framework64", "v2.0.50727", "Temporary ASP.NET Files"),
                Path.Combine(systemRoot, "Microsoft.NET", "Framework", "v2.0.50727", "Temporary ASP.NET Files"),
                // Consider adding paths for .NET Core if applicable, though typically handled differently
            };

            foreach (var dirPath in tempDirectories)
            {
                try
                {
                    if (string.IsNullOrEmpty(dirPath)) continue; // Skip if path construction failed
                    DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                    if (!dirInfo.Exists)
                    {
                        Console.WriteLine($"[CacheScanner] Temporary directory not found: {dirPath}");
                        continue;
                    }

                    Console.WriteLine($"[CacheScanner] Scanning directory: {dirPath}");
                    foreach (FileInfo file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    {
                        if (file.Length > largeFileThreshold)
                        {
                            Console.WriteLine($"[CacheScanner] Large temporary file found: {file.FullName}, Size: {file.Length / (1024 * 1024)} MB");
                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"[CacheScanner] Access denied to directory: {dirPath}. Error: {ex.Message}");
                }
                catch (DirectoryNotFoundException ex)
                {
                    Console.WriteLine($"[CacheScanner] Directory not found during scan: {dirPath}. Error: {ex.Message}");
                }
                catch (Exception ex) // Catch other potential exceptions
                {
                    Console.WriteLine($"[CacheScanner] Error scanning directory {dirPath}: {ex.Message}");
                }
            }
            Console.WriteLine("[CacheScanner] Finished scanning for large temporary files.");
        }

        private static void ScanForCorruptCache()
        {
            Console.WriteLine("[CacheScanner] Scanning for corrupt cache files/directories...");
            string systemDrive = Environment.GetEnvironmentVariable("SystemDrive");

            List<string> cacheDirectories = new List<string>
            {
                Path.Combine(systemDrive, "inetpub", "temp", "IIS Temporary Compressed Files"),
                // Add other known output cache or application-specific cache directories here
                // For example, if an application uses a specific folder for its cache:
                // Path.Combine(systemDrive, "MyApplicationCache")
            };

            foreach (var dirPath in cacheDirectories)
            {
                try
                {
                    if (string.IsNullOrEmpty(dirPath)) continue; // Skip if path construction failed
                    DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                    if (!dirInfo.Exists)
                    {
                        Console.WriteLine($"[CacheScanner] Cache directory not found: {dirPath}");
                        continue;
                    }

                    Console.WriteLine($"[CacheScanner] Scanning cache directory for corruption: {dirPath}");

                    // Check for unexpectedly empty directories that might indicate issues
                    if (!dirInfo.EnumerateFileSystemInfos().Any())
                    {
                        // Be cautious with this check; some cache directories might be legitimately empty.
                        // This is a heuristic. Perhaps log as a warning or informational.
                        Console.WriteLine($"[CacheScanner] Warning: Cache directory is empty: {dirPath}. This might be normal or indicate an issue.");
                    }

                    // Check for zero-byte files that were recently modified
                    // Such files might indicate incomplete writes or corruption.
                    foreach (FileInfo file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    {
                        if (file.Length == 0 && (DateTime.UtcNow - file.LastWriteTimeUtc) < TimeSpan.FromDays(1)) // Recently modified
                        {
                            Console.WriteLine($"[CacheScanner] Suspicious zero-byte file found (recent modification): {file.FullName}");
                        }
                        // Add other corruption checks here, e.g., specific file patterns, magic numbers, etc.
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"[CacheScanner] Access denied to cache directory: {dirPath}. Error: {ex.Message}");
                }
                catch (DirectoryNotFoundException ex)
                {
                    Console.WriteLine($"[CacheScanner] Cache directory not found during scan: {dirPath}. Error: {ex.Message}");
                }
                catch (Exception ex) // Catch other potential exceptions
                {
                    Console.WriteLine($"[CacheScanner] Error scanning cache directory {dirPath}: {ex.Message}");
                }
            }
            Console.WriteLine("[CacheScanner] Finished scanning for corrupt cache files/directories.");
        }
    }
}

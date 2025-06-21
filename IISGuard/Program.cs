using System;
using System.Reflection.Metadata;
using IISGuard.Services;

namespace IISGuard
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("IIS Guard CLI - Auto-Heal & Config Validator");
            AppPoolChecker.CheckAppPools();
            BindingValidator.ValidateBindings();
            CacheScanner.ScanCacheHealth();
            Console.WriteLine("Checks completed. Review output above.");
        }
    }
}